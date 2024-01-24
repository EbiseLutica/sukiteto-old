using Promete;
using Promete.Audio;
using Promete.GLDesktop;
using Promete.Input;
using Sukiteto;

var app = PrometeApp.Create()
    .Use<Keyboard>()
    .Use<ConsoleLayer>()
    .Use<KeyConfigService>()
    .Use<AudioPlayer>()
    .Use<Resources>()
    .Use<ShapeLoader>()
    .Use<GameService>()
    .BuildWithOpenGLDesktop();

return app.Run<TitleScene>();
