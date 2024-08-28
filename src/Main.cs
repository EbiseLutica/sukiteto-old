using Promete;
using Promete.Audio;
using Promete.GLDesktop;
using Promete.Input;
using Quadrix;

var app = PrometeApp.Create()
    .Use<Keyboard>()
    .Use<Mouse>()
    .Use<Gamepads>()
    .Use<ConsoleLayer>()
    .Use<InputService>()
    .Use<AudioPlayer>()
    .Use<Resources>()
    .Use<UIService>()
    .BuildWithOpenGLDesktop();

return app.Run<TitleScene>();
