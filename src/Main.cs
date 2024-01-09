using DotFeather;
using Sukiteto;

DF.Window.Start += () =>
{
    DF.Window.Title = "Sukiteto";
    DF.Window.Size = (640, 480);
    
    Global.Initialize();
};

return DF.Run<TitleScene>();
