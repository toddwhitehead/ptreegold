using Terminal.Gui;
using PTreeGold.Services;
using PTreeGold.UI;

var configService = new ConfigService();
var settings = configService.Load();

var generator  = new PasswordGenerator();
var entropy    = new EntropyCalculator();
var export     = new ExportService();

Application.Init();
try
{
    var win = new MainWindow(settings.Patterns, generator, entropy, export);
    Application.Run(win);
}
finally
{
    Application.Shutdown();
}
