# RaspistreamNetCore

 

**Prerequisites**

A Raspberry Pi 3.
A camera connected to the Pi (and enabled).
Net.Core 2.1 installed on the Pi and on the build computer.

**Compile and publish for Raspberry Pi**

On the build computer with Visual Code or terminal:

dotnet publish -c Release -r linux-arm RaspiCam.sln

 

Copy the entire RaspiStream/bin/Release/netcoreapp2.1/linux-arm/publish directory to your Raspberry Pi (eg in /home/your username/ raspistream).

Edit /etc/hosts and add a line "Your external IP" name (where name is the name of your Pi on your network)

*Run*

Run ./RaspiStream

 
You should be able to see the video stream using VLC or ffmplay by opening the network stream "rtsp://"name":8554/picamera" or "rtsp://"X.X.X.X":8554/picamera" (the Pi ip address).
