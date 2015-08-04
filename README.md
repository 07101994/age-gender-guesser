# Age & Gender guesser for Windows 10

### What is this?
This is a Windows 10 app that will guess the age and gender of anybody in a photo. It uses the [Project Oxford API](https://www.projectoxford.ai/) for face recognition and age/gender guessing. It uses [Win2D](https://github.com/microsoft/win2d) for its graphics rendering.

### Why is this interesting?
This app shows a simple use-case of Win2D and the new [camera interop](https://msdn.microsoft.com/library/windows.media.effects.ibasicvideoeffect) available in Windows 10. It runs on the Universal Windows Platform, so works on desktop PCs, phones, and much more!

It also demonstrates how to use Project Oxford in a Windows 10 application.

### How do I run it?

Clone this repository and open Win2D-Face.sln in Visual Studio 2015.

You will need to get an API key for Project Oxford. Instructions are available [here](https://www.projectoxford.ai/face). Make sure you update the 'faceServiceClient' variable in MainPage.xaml.cs to use your own key.

### What license is this available under?

Apache 2.0.

### Can I see it in action?

![:( no Satya image found](https://github.com/austinkinross/how-old-windows10/blob/master/examples/satya_sample.png?raw=true)

### Is this in any way official?

Nope. This was part of an 'app week' for our team at Microsoft. It was written in a few hours, so the code quality is questionable.
