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
    get_processes()
    network_select_1.grid()
    network_select_2.grid()
    root.mainloop()
    
if __name__ == "__main__":
    main()