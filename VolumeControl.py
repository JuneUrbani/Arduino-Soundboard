from pycaw.pycaw import AudioUtilities
import serial
import time

# Serial port (Should make this automatically find the Arduino?)
ser = serial.Serial('COM3',9600)
time.sleep(2)

# Analog volume reads from 0 to 438
max_analog_value = 438

# Channel 1 variables
target_1 = 'chrome.exe'
mute_1_locked = False
mute_1_swap = False

# Channel 2 variables
target_2 = 'steam.exe'
mute_2_locked = False
mute_2_swap = False

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
                print(self.process_name, 'has been muted.')  # debug

    def unmute(self):
        sessions = AudioUtilities.GetAllSessions()
        for session in sessions:
            interface = session.SimpleAudioVolume
            if session.Process and session.Process.name() == self.process_name:
                interface.SetMute(0, None)
                print(self.process_name, 'has been unmuted.')  # debug

    def process_volume(self):
        sessions = AudioUtilities.GetAllSessions()
        for session in sessions:
            interface = session.SimpleAudioVolume
            if session.Process and session.Process.name() == self.process_name:
                print('Volume:', interface.GetMasterVolume())  # debug
                return interface.GetMasterVolume()

    def set_volume(self, decibels):
        sessions = AudioUtilities.GetAllSessions()
        for session in sessions:
            interface = session.SimpleAudioVolume
            if session.Process and session.Process.name() == self.process_name:
                # only set volume in the range 0.0 to 1.0
                self.volume = min(1.0, max(0.0, decibels))
                interface.SetMasterVolume(self.volume, None)
                print('Volume set to', self.volume)  # debug

    def decrease_volume(self, decibels):
        sessions = AudioUtilities.GetAllSessions()
        for session in sessions:
            interface = session.SimpleAudioVolume
            if session.Process and session.Process.name() == self.process_name:
                # 0.0 is the min value, reduce by decibels
                self.volume = max(0.0, self.volume-decibels)
                interface.SetMasterVolume(self.volume, None)
                print('Volume reduced to', self.volume)  # debug

    def increase_volume(self, decibels):
        sessions = AudioUtilities.GetAllSessions()
        for session in sessions:
            interface = session.SimpleAudioVolume
            if session.Process and session.Process.name() == self.process_name:
                # 1.0 is the max value, raise by decibels
                self.volume = min(1.0, self.volume+decibels)
                interface.SetMasterVolume(self.volume, None)
                print('Volume raised to', self.volume)  # debug

def main():
    # We will have to set up multiple audio controllers, but as long as we know the target we can
    # use the same function for each channel
    global target_1
    global target_2
    audio_controller_1 = AudioController(target_1)
    audio_controller_2 = AudioController(target_2)
    #ser.read_until('\r')
    while True:
        byte_str = str(ser.readline())
        split_str = byte_str[2:][:-5].split('-')
        print(split_str)
        
        if len(split_str) == 4:
            if split_str[0] == '0':
                audio_controller_1.unmute()
            else:
                audio_controller_1.mute()
            audio_controller_1.set_volume(int(split_str[1]) / max_analog_value)
            
            if split_str[2] == '0':
                audio_controller_1.unmute()
            else:
                audio_controller_1.mute()
            audio_controller_2.set_volume(int(split_str[3]) / max_analog_value)
        
        time.sleep(0.05)

if __name__ == "__main__":
    main()