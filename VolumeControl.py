from pycaw.pycaw import AudioUtilities
import serial
import time
import wmi
import tkinter as tk

process_list = ['']

root = tk.Tk()

canvas1 = tk.Canvas(root, width = 500, height = 300)

channel_1_option = tk.StringVar(root)
channel_2_option = tk.StringVar(root)

network_select_1 = tk.OptionMenu(root, channel_1_option, *process_list)
network_select_2 = tk.OptionMenu(root, channel_2_option, *process_list)

canvas1.grid()

def update_process_list():  
    label1 = tk.Label(root, text= 'Updated!', fg='green', font=('helvetica', 12, 'bold'))
    get_processes()

update_button = tk.Button(text='Update Process List',command=update_process_list, bg='brown',fg='white')
canvas1.create_window(150, 150, window=update_button)

# Serial port (Should make this automatically find the Arduino?)
ser = serial.Serial('COM3',9600)
time.sleep(2)

# Analog volume reads from 0 to this
max_analog_value = 1023

# Channel 1 variables
target_1 = 'chrome.exe'

# Channel 2 variables
target_2 = 'steam.exe'

class AudioController(object):
    def __init__(self, process_name):
        self.process_name = process_name
        self.volume = self.process_volume()

    def mute(self):
        sessions = AudioUtilities.GetAllSessions()
        for session in sessions:
            interface = session.SimpleAudioVolume
            if session.Process and session.Process.name() == self.process_name:
                interface.SetMute(1, None)
                #print(self.process_name, 'has been muted.')  # debug

    def unmute(self):
        sessions = AudioUtilities.GetAllSessions()
        for session in sessions:
            interface = session.SimpleAudioVolume
            if session.Process and session.Process.name() == self.process_name:
                interface.SetMute(0, None)
                #print(self.process_name, 'has been unmuted.')  # debug

    def process_volume(self):
        sessions = AudioUtilities.GetAllSessions()
        for session in sessions:
            interface = session.SimpleAudioVolume
            if session.Process and session.Process.name() == self.process_name:
                #print('Volume:', interface.GetMasterVolume())  # debug
                return interface.GetMasterVolume()

    def set_volume(self, decibels):
        sessions = AudioUtilities.GetAllSessions()
        for session in sessions:
            interface = session.SimpleAudioVolume
            if session.Process and session.Process.name() == self.process_name:
                # only set volume in the range 0.0 to 1.0
                self.volume = min(1.0, max(0.0, decibels))
                interface.SetMasterVolume(self.volume, None)
                #print('Volume set to', self.volume)  # debug

    def decrease_volume(self, decibels):
        sessions = AudioUtilities.GetAllSessions()
        for session in sessions:
            interface = session.SimpleAudioVolume
            if session.Process and session.Process.name() == self.process_name:
                # 0.0 is the min value, reduce by decibels
                self.volume = max(0.0, self.volume-decibels)
                interface.SetMasterVolume(self.volume, None)
                #print('Volume reduced to', self.volume)  # debug

    def increase_volume(self, decibels):
        sessions = AudioUtilities.GetAllSessions()
        for session in sessions:
            interface = session.SimpleAudioVolume
            if session.Process and session.Process.name() == self.process_name:
                # 1.0 is the max value, raise by decibels
                self.volume = min(1.0, self.volume+decibels)
                interface.SetMasterVolume(self.volume, None)
                #print('Volume raised to', self.volume)  # debug

def get_processes():
    global channel_1_option
    global channel_2_option
    global network_select_1
    global network_select_2
    
    channel_1_option.set('')
    channel_2_option.set('')
    network_select_1['menu'].delete(0, 'end')
    network_select_2['menu'].delete(0, 'end')
    
    global process_list
    f = wmi.WMI()
    
    process_list = []
      
    for process in f.Win32_Process():
        process_list.append(process.Name)

    process_set = set(process_list)
    
    process_list = sorted(process_set, key=str.lower)
    
    for choice in process_list:
        network_select_1['menu'].add_command(label=choice, command=tk._setit(channel_1_option, choice))
        network_select_2['menu'].add_command(label=choice, command=tk._setit(channel_2_option, choice))

def main():
    # Set up GUI
    get_processes()
    network_select_1.grid()
    network_select_2.grid()
    #root.mainloop()
    
    # We will have to set up multiple audio controllers, but as long as we know the target we can
    # use the same function for each channel
    global target_1
    global target_2
    global channel_1_option
    global channel_2_option
    #ser.read_until('\r')
    while True:
        root.update()
        target_1 = channel_1_option.get()
        target_2 = channel_2_option.get()
        audio_controller_1 = AudioController(target_1)
        audio_controller_2 = AudioController(target_2)
        
        byte_str = str(ser.readline())
        split_str = byte_str[2:][:-5].split('-')
        #print(split_str)
        
        if len(split_str) == 4:
            if split_str[0] == '1':
                audio_controller_1.mute()
            else:
                audio_controller_1.unmute()
            audio_controller_1.set_volume((max_analog_value - int(split_str[1])) / max_analog_value)
            
            if split_str[2] == '1':
                audio_controller_2.mute()
            else:
                audio_controller_2.unmute()
            audio_controller_2.set_volume((max_analog_value - int(split_str[3])) / max_analog_value)
        
        time.sleep(0.025)

if __name__ == "__main__":
    main()