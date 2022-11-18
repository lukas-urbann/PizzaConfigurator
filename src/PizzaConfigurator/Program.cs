using Spectre.Console;

namespace PizzaConfigurator
{
    public enum BasePizza
    {
        Salame,
        Hawaii,
        Milano,
        Capricciosa,
        QuattroFormaggi,
        Mexicana,
        Vegetaria,
    }
    
    public enum Ingredients
    {
        Tomato,
        Lettuce,
        Anchovies,
        Cheese,
        Salami,
        Pepperoni,
        Onion,
        //Pineapple, Ananas na pizzu nepatří, takže nemá cenu ho do zde uvádět
        Ketchup,
        Butter,
    }
    
    struct Pizza
    {
        public int Id;
        public bool Favorite;
        public BasePizza PizzaType;
        public List<Ingredients> IngredientsList;

        public void ChangeBase(BasePizza type)
        {
            PizzaType = type;
        }
        
        public void AddIngredient(Ingredients ingredient)
        {
            IngredientsList ??= new List<Ingredients>();

            IngredientsList.Add(ingredient);
        }
        
        public void RemoveIngredient(int index)
        {
            if (IngredientsList == null)
                return;

            IngredientsList.RemoveAt(index);
        }
    }
    
    internal class Program
    {
        private const string ExceptionFolderName = "Exceptions";
        private const string OrdersFolderName = "Orders";
        private const int IdLength = 9;
        private Pizza _editedPizza;
        
        static void Main()
        {
            Program main = new Program();

            main.Runtime();
            main.ApplicationExit();
        }

        private void Runtime()
        {
            WriteIntro();
            MainMenu();
            ApplicationExit();
        }

        private void MainMenu()
        {
            while (true)
            {
                Console.Clear();
                AnsiConsole.Markup("[underline green]Main Menu[/]\n");
                Console.WriteLine("M - Create a new pizza");
                Console.WriteLine("E - Edit pizza");
                Console.WriteLine("F - Display pizza order list");
                Console.WriteLine("D - Delete a pizza");
                Console.WriteLine("X - Exit");

                char input = Char.ToLower(Console.ReadKey().KeyChar);

                switch (input)
                {
                    case 'x':
                        return;
                    case 'm':
                        GenerateOrder();
                        break;
                    case 'd':
                        DeletePizza();
                        break;
                    case 'f':
                        ListAllPizzas();
                        break;
                    case 'e':
                        PizzaEditor();
                        break;
                }
            }
        }

        private void DeletePizza()
        {
            while (true)
            {
                Console.Clear();
                AnsiConsole.Markup("[underline red]Pizza order deleter[/]\n");
                int[] orderIdList = GetOrderIdList();

                for (int i = 0; i < orderIdList.Length; i++)
                    Console.WriteLine(i+1 + ". " + orderIdList[i]);
                
                AnsiConsole.Markup("[underline yellow]Use the ordered ID number to refer to your pizza. Type 'x' to exit.[/]\n");

                string? val = Console.ReadLine();
            
                try
                {
                    if(val != null && val.ToLower().Equals("x"))
                        return;
                    
                    var select = Convert.ToInt32(val);
                    select -= 1;
                    
                    DeleteOrderFile(orderIdList[select]);
                }
                catch (Exception ex)
                {
                    ExceptionOutput(ex);
                }
            }
        }

        private void DeleteOrderFile(int id)
        {
            int[] orderIdList = GetOrderIdList();

            foreach (var t in orderIdList)
                if (t == id)
                    if (File.Exists(GenerateDirectoryString(OrdersFolderName) + id + ".order"))
                        File.Delete(GenerateDirectoryString(OrdersFolderName) + id + ".order");
        }

        private void WriteIntro()
        {
            AnsiConsole.Markup("[underline green]Welcome to PizzaConfigurator![/]\n" + "Press a key to continue...\n");
            Console.ReadKey();
            Console.WriteLine();
        }

        private void GenerateOrder()
        {
            int orderId = GenerateId();
            Pizza orderedPizza = GeneratePizza();
            orderedPizza.Id = orderId;
            PizzaEditor(orderedPizza);
        }

        private void PizzaEditor()
        {
            if(CheckFolder(OrdersFolderName))
                CreateFolder(OrdersFolderName);
            
            while (true)
            {
                Console.Clear();
                AnsiConsole.Markup("[underline green]Pizza order chooser[/]\n");
                int[] orderIdList = GetOrderIdList();

                for (int i = 0; i < orderIdList.Length; i++)
                    Console.WriteLine(i+1 + ". " + orderIdList[i]);
                
                AnsiConsole.Markup("[underline yellow]Use the ordered ID number to refer to your pizza. Type 'x' to exit.[/]\n");

                string? val = Console.ReadLine();
            
                try
                {
                    if(val != null && val.ToLower().Equals("x"))
                        return;
                    
                    var select = Convert.ToInt32(val);
                    select -= 1;
                    
                    LoadOrderFile(orderIdList[select]);
                    break;
                }
                catch (Exception ex)
                {
                    ExceptionOutput(ex);
                }
            }

            Console.WriteLine("Loaded pizza:");
            ReadPizzaOrder(true);
            PizzaEditor(_editedPizza);
        }

        private void PizzaEditor(Pizza pizza)
        {
            _editedPizza = pizza;
            
            while (true)
            {
                Console.Clear();
                AnsiConsole.Markup("[underline green]Pizza creation menu[/]\n");
                Console.WriteLine("A - Add ingredients");
                Console.WriteLine("R - Remove ingredients");
                Console.WriteLine("P - Change pizza type");
                Console.WriteLine("F - Favorite/Unfavorite a pizza");
                Console.WriteLine("S - Save your pizza");
                Console.WriteLine("I - Display pizza attributes");
                Console.WriteLine("X - Go back to Main Menu (All changes will be lost without saving)");
                
                char input = Char.ToLower(Console.ReadKey().KeyChar);

                switch (input)
                {
                    case 'a':
                        AddIngredient();
                        break;
                    case 'r':
                        RemoveIngredient();
                        break;
                    case 'p':
                        ChangePizzaType();
                        break;
                    case 'f':
                        SetPizzaFavorite();
                        break;
                    case 's':
                        CreateOrder(_editedPizza);
                        break;
                    case 'i':
                        ReadPizzaOrder(true);
                        break;
                    case 'x':
                        return;
                }
            }
        }

        private void ListAllPizzas()
        {
            Console.Clear();
            
            int[] ids = GetOrderIdList();

            if (ids.Length <= 0)
            {
                Console.WriteLine("There are no orders yet.");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
            
            for (int i = 0; i < ids.Length; i++)
            {
                Console.Clear();
                LoadOrderFile(ids[i]);
                ReadPizzaOrder(false);
                Console.WriteLine("-------------");
                Console.WriteLine("Pizza " + (i+1) + "/" + (ids.Length));
                Console.WriteLine("Press a key to continue...");
                if(i < ids.Length)
                    Console.ReadKey();
            }

            Console.WriteLine("\nEnd - Press a key to continue...");
            Console.ReadKey();
        }

        private int GenerateId()
        {
            Random r = new Random();
            int generatedId;
            int minInt = 1;
            int maxInt = 0;
            int[] otherIds = GetOrderIdList();

            for (int i = 0; i < IdLength; i++)
            {
                maxInt += (int) (9 * Math.Pow(10, i));
                minInt *= 10;
            }
            
            do
            {
                generatedId = r.Next(minInt/10, maxInt);
            } while (CheckIdAvailability(otherIds, generatedId));

            return generatedId;
        }

        private bool CheckIdAvailability(int[] idList, int generatedId)
        {
            foreach (var t in idList)
                if (t == generatedId)
                    return true;

            return false;
        }

        private int[] GetOrderIdList()
        {
            if(CheckFolder(OrdersFolderName))
                CreateFolder(OrdersFolderName);
            
            string[] orderIdList = Directory.GetFiles(GenerateDirectoryString(OrdersFolderName));
            int[] intOrderIdList = new int[orderIdList.Length];

            for (int i = 0; i < orderIdList.Length; i++)
            {
                orderIdList[i] = orderIdList[i].Replace(GenerateDirectoryString(OrdersFolderName), "");
                orderIdList[i] = orderIdList[i].Remove(IdLength);
                intOrderIdList[i] = Convert.ToInt32(orderIdList[i]);
            }

            return intOrderIdList;
        }

        private void ReadPizzaOrder(bool keyPress)
        {
            Console.Clear();
            Console.WriteLine("ID: " + _editedPizza.Id);

            switch (_editedPizza.Favorite)
            {
                case true:
                    Console.WriteLine("Pizza is favorite");
                    break;
                case false:
                    Console.WriteLine("Pizza isn't marked as favorite");
                    break;
            }
            
            Console.WriteLine("Base pizza type: " + _editedPizza.PizzaType);
                

            if (_editedPizza.IngredientsList != null)
            {
                if (_editedPizza.IngredientsList.Count != 0)
                {
                    Console.WriteLine("Additional pizza ingredients: ");
                
                    for (int i = 0; i < _editedPizza.IngredientsList.Count; i++)
                        Console.WriteLine("  " + (i+1) + ". " + _editedPizza.IngredientsList[i]);
                }
            }
            else
            {
                Console.WriteLine("None additional pizza ingredients.");
            }
                

            if (keyPress)
            {
                Console.WriteLine("\nPress a key to continue...");
                Console.ReadKey();
            }
        }
        
        private void LoadOrderFile(int id)
        {
            int[] orderIdList = GetOrderIdList();

            foreach (var t in orderIdList)
                if (t == id)
                    if (File.Exists(GenerateDirectoryString(OrdersFolderName) + id + ".order"))
                        LoadPizzaFromOrder(File.ReadAllLines(GenerateDirectoryString(OrdersFolderName) + id + ".order"));
        }

        private void LoadPizzaFromOrder(string[] pizzaInfo)
        {
            _editedPizza = new Pizza
            {
                Id = Int32.Parse(pizzaInfo[2]),
                Favorite = Convert.ToBoolean(pizzaInfo[0].Split(';')[0]),
                PizzaType = (BasePizza)Enum.Parse(typeof(BasePizza), pizzaInfo[0].Split(';')[1])
            };

            string[] ingreds = pizzaInfo[1].Split(';');
            
            for (int i = 0; i < ingreds.Length-1; i++)
                _editedPizza.AddIngredient((Ingredients)Enum.Parse(typeof(Ingredients), ingreds[i]));
        }

        private void CreateOrder(Pizza pizza)
        {
            string ingredients = "";

            if(pizza.IngredientsList != null)
                foreach (var t in pizza.IngredientsList)
                    ingredients += "" + t + ";";

            string[] fileInput =
            {
                pizza.Favorite + ";" + pizza.PizzaType,
                ingredients,
                pizza.Id.ToString()
            };
            
            if(CheckFolder(OrdersFolderName))
                CreateFolder(OrdersFolderName);
            
            File.WriteAllLinesAsync(GenerateDirectoryString(OrdersFolderName) + pizza.Id + ".order", fileInput);
        }

        private Pizza GeneratePizza()
        {
            Pizza pizza = new Pizza();
            
            

            return pizza;
        }

        private void AddIngredient()
        {
            var ingredientsList = Enum.GetValues(typeof(Ingredients)).Cast<Ingredients>().ToList();
            
            while (true)
            {
                Console.Clear();
                Console.WriteLine("You can add these ingredients:");

                for (int i = 0; i < ingredientsList.Count; i++)
                    Console.WriteLine(i+1 + ". " + ingredientsList[i]);
                    
                Console.WriteLine("Write ingredient ID equivalent to your selection. Or type 'X' to go back.");

                string? val = Console.ReadLine();
            
                try
                {
                    if(val != null && val.ToLower().Equals("x"))
                        return;
                    
                    var select = Convert.ToInt32(val);
                    _editedPizza.AddIngredient(ingredientsList[select - 1]);
                }
                catch (Exception ex)
                {
                    ExceptionOutput(ex);
                }
            }
        }

        private void ExceptionOutput(Exception ex)
        {
            if(CheckFolder(ExceptionFolderName))
                CreateFolder(ExceptionFolderName);
            
            File.WriteAllLinesAsync(GenerateExceptionFileName(ex), new[] { ex.Message + "\n" + ex.StackTrace });
        }
        
        private string GenerateExceptionFileName(Exception ex)
        {
            return GenerateDirectoryString(ExceptionFolderName) + GenerateCurrentDate() + " - " + ex.Message + ".txt";
        }

        private string GenerateCurrentDate()
        {
            return DateTime.Now.ToString("dd/MM/yyyy") +
                   " " +
                   DateTime.Now.ToString("T").Replace(':', '.');
        }

        private string GenerateDirectoryString(string additionalFolder)
        {
            return Directory.GetCurrentDirectory() + "\\" + additionalFolder + "\\";
        }

        private bool CheckFolder(string folderName)
        {
            string currentDirectoryPath = Directory.GetCurrentDirectory() + "/" + folderName;

            if (Directory.Exists(currentDirectoryPath))
                return false;
            
            return true;
        }

        private void CreateFolder(string folderName)
        {
            Directory.CreateDirectory(folderName);
        }
        
        private void RemoveIngredient()
        {
            if (_editedPizza.IngredientsList == null)
            {
                Console.Clear();
                Console.WriteLine("This pizza doesn't have any ingredients yet.");
                Console.WriteLine("Press a key to continue...");
                Console.ReadKey();
                return;
            }
            
            while (true)
            {
                Console.Clear();
                Console.WriteLine("You can remove these ingredients:");

                for (int i = 0; i < _editedPizza.IngredientsList.Count; i++)
                    Console.WriteLine(i+1 + ". " + _editedPizza.IngredientsList[i]);
                    
                Console.WriteLine("Write ingredient ID equivalent to your selection. Or type 'X' to go back.");

                string? val = Console.ReadLine();
            
                try
                {
                    if(val != null && val.ToLower().Equals("x"))
                        return;
                    
                    var select = Convert.ToInt32(val);
                    _editedPizza.RemoveIngredient(select-1);
                }
                catch (Exception ex)
                {
                    ExceptionOutput(ex);
                }
            }
        }
        
        private void ChangePizzaType()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Select your pizza base:");

                BasePizza[]? basePizzas = Enum.GetValues(typeof(BasePizza)) as BasePizza[];

                if (basePizzas != null)
                    for (int i = 0; i < basePizzas.Length; i++)
                        Console.WriteLine((i + 1) + ". " + basePizzas[i]);

                Console.WriteLine("Write pizza base ID equivalent to your selection. Or type 'X' to go back.");

                string? val = Console.ReadLine();
            
                try
                {
                    if(val != null && val.ToLower().Equals("x"))
                        return;
                    
                    var select = Convert.ToInt32(val);
                    if (basePizzas != null) _editedPizza.ChangeBase(basePizzas[select - 1]);
                    break;
                }
                catch (Exception ex)
                {
                    ExceptionOutput(ex);
                }
            }
        }
        
        private void SetPizzaFavorite()
        {
            _editedPizza.Favorite = !_editedPizza.Favorite;
        }
        
        private void ApplicationExit()
        {
            Console.Clear();
            
            AnsiConsole.Markup("[underline green]Thanks for your visit![/]\n" + "Press a key to continue...\n");
            Console.ReadKey();
        }
    }
}