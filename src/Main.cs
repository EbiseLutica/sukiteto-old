using DotFeather;
using Sukiteto;

DF.Window.Start += () =>
{
    DF.Window.Title = "Sukiteto";
    DF.Window.Size = (640, 480);
    
    Global.Initialize();
};

var status = DF.Run<TitleScene>();
Global.Keys.Save();
return status;
