using Microsoft.Extensions.Configuration;
using NLog;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace SMS.WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IConfiguration _configuration;
        private readonly ObservableCollection<EnvironmentVariable> _variables;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
            InitializeComponent();

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            _variables = LoadEnvironmentVariables();
            environmentVariablesGrid.ItemsSource = _variables;
        }

        private ObservableCollection<EnvironmentVariable> LoadEnvironmentVariables()
        {
            var variableNames = _configuration.GetSection("EnvironmentVariables").Get<List<string>>();
            var variables = new ObservableCollection<EnvironmentVariable>();

            foreach (var name in variableNames)
            {
                var value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User) ?? "Значение по умолчанию";
                variables.Add(new EnvironmentVariable { Field = name, Value = value, Comment = "" });
            }
            Logger.Info($"Получено {variables.Count} переменных");
            return variables;
        }

        private void SaveEnvironmentVariables(IEnumerable<EnvironmentVariable> variables)
        {
            foreach (var variable in variables)
            {
                try
                {
                    Environment.SetEnvironmentVariable(variable.Field, variable.Value, EnvironmentVariableTarget.User);
                    Logger.Info($"Переменная '{variable.Field} = {variable.Value}' сохранена");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при сохранении переменной '{variable.Field} = {variable.Value}'");
                    Logger.Error($"Ошибка при сохранении переменной '{variable.Field} = {variable.Value}'\n" + ex.Message);
                }
            }
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveEnvironmentVariables(_variables);
        }

        //TODO Доработать добавление переменных, добавить проверки полей
        private void environmentVariablesGrid_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "Поле" && e.Row.Item is EnvironmentVariable newVariable)
            {
                if (string.IsNullOrWhiteSpace(newVariable.Field))
                {
                    return;
                }

                if (newVariable.Value == null)
                {
                    newVariable.Value = "Значение по умолчанию";
                }

            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}