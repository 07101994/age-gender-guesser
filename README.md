# Age & Gender guesser for Windows 10

### What is this?
Use this to find out somebody's age and gender! Simply take a photograph using your Windows 10 device's camera, and for each person in the photograph the app will guess their age and gender and draw it above their face. It is very similar to www.how-old.net.

This is a Windows 10 application written for the Universal Windows Platform. It uses the [Project Oxford API](https://www.projectoxford.ai/) for face recognition and age/gender guessing. It uses [Win2D](https://github.com/microsoft/win2d) for its graphics rendering. It makes use of the new [camera video effects](https://msdn.microsoft.com/library/windows.media.effects.ibasicvideoeffect) added to Windows 10, and the interop available between the camera and Win2D.

### How do I run it?

Clone this repository and open Win2D-Face.sln in Visual Studio 2015.

You will need to get an API key for Project Oxford. Instructions are available [here](https://www.projectoxford.ai/face). Make sure you update the 'faceServiceClient' variable in MainPage.xaml.cs to use your own key.

### What license is this available under?

Apache 2.0.

### Can I see it in action?

![:( no Satya image found](https://github.com/austinkinross/how-old-windows10/blob/master/examples/satya_sample.png?raw=true)

### Is this in any way official?

Nope. This was part of an 'app week' for our team at Microsoft. It was written in a few hours, so the code quality is questionable.
