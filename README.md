# Application to flash subliminal messages on screen

## Summary:

This application runs hidden in the desktop, and flashes subliminal 
 messages on the screen. With this application, you can also: 

* Encrypt messages on disk
* Pause and resume messages using hotkeys
* Configure the time duration for which messages will be flashed
* Configure the time duration between flashes
* Configure the font (type and color)
* Change the rate at which messages are displayed

## Compilation:

Please execute the following commands:

```
C:\SubMsgs> compile.bat
```  

## Execution:

The following executables are created upon compilation:

* subMsgs.exe
* encryptMsgs.exe
* decryptMsgs.exe

Following is the procedure to execute the messages:

* Edit configuration.conf and change the following: 
  * delay_time: Time between flashes (time in ms)
  * blink_time: Time for which messages are flashed (time in ms)
  * key: Encryption key (make it an 8 character word of your choice, 
         lets say *test1234*)
  * font_size: Font size of displayed messages
  * font_type: Font type of displayed messages
  * font_color: Color of displayed messages
  * msg_display_speed: Either "normal" or "slowing", or "faster" 
                       (without the quotes)

* Type in the messages in a file named messages.txt (using notepad)
* Encrypt messages.txt (clear text file) to messages.smsg 
   (encrypted file) using the following command: 
```
C:\SubMsgs> encryptMsgs.exe messages.txt messages.smsg test1234
```  

* Execute subMsgs.exe
* Select configuration.conf as the configuration file
* Click on Load Messages
* Select the messages.smsg file
* Click on Clear Window

Once launched, following are the hotkeys 
* Shift+Alt+D - Show/hide the configuration screen
* Shift+Alt+E - Pause/resume the messages displayed

If you want to edit/make changes to the messages, press Shift+Alt+E, 
 click on Quit, and execute the following commands on a terminal:

```
C:\SubMsgs> decryptMsgs.exe messages.smsg messages.txt test123							
C:\SubMsgs> notepad messages.txt
C:\SubMsgs> encryptMsgs.exe messages.txt messages.smsg test123
```

After this, double click on subMsgs.exe and follow the 
 steps described above again
