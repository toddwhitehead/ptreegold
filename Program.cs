using PwdGen.Services;
using PwdGen.UI;

var configService = new ConfigService();
var settings = configService.Load();

var generator = new PasswordGenerator();
var entropy = new EntropyCalculator();
var export = new ExportService();

var generateFlow = new GenerateFlow(generator, entropy, export, settings.Patterns);
var menu = new MainMenu(generateFlow, settings.Patterns);

await menu.RunAsync();
