using System;
using System.Collections.Generic;
using System.Linq;

namespace DroneDashApp
{
    enum DroneStatus { Inactive, Active, Delivery }
    enum OrderStatus { New, InDelivery, Completed, Rejected }

    interface IOrderGenerator
    {
        Order GenerateOrder();
    }

    interface IDroneManager
    {
        Drone AddDrone(string name);
        bool AssignDrone(int droneId, Order order);
        bool ChangeDroneStatus(int droneId, DroneStatus status);
        List<Drone> ListDrones(DroneStatus? filter = null);
    }

    abstract class Person
    {
        public string FirstName { get; }
        public string LastName { get; }
        protected Person(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }

    sealed class Customer : Person
    {
        public Customer(string firstName, string lastName)
            : base(firstName, lastName) { }
    }

    class Order
    {
        private static int NextId = 1;
        public int Id { get; }
        public Customer Customer { get; }
        public string City { get; }
        public string Street { get; }
        public Drone AssignedDrone { get; set; }
        public OrderStatus Status { get; set; }

        public Order(Customer customer, string city, string street)
        {
            Id = NextId++;
            Customer = customer;
            City = city;
            Street = street;
            Status = OrderStatus.New;
        }
    }

    class Drone
    {
        public int Id { get; }
        public string Name { get; }
        public DroneStatus Status { get; set; }

        public Drone(int id, string name)
        {
            if (id <= 0)
                throw new ArgumentException("ID musi być dodatnie.");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nazwa nie może być pusta.");
            Id = id;
            Name = name;
            Status = DroneStatus.Inactive;
        }
    }

    class DroneManager : IDroneManager
    {
        private readonly List<Drone> drones = new List<Drone>();
        private int nextDroneId = 1;

        public DroneManager()
        {
            drones.Add(new Drone(nextDroneId++, "Bob"));
            drones.Add(new Drone(nextDroneId++, "Rick"));
        }

        public Drone AddDrone(string name)
        {
            var dr = new Drone(nextDroneId++, name);
            drones.Add(dr);
            Console.WriteLine($"Dodano drona: {dr.Name} (ID: {dr.Id})");
            return dr;
        }

        public bool AssignDrone(int droneId, Order order)
        {
            var dr = drones.FirstOrDefault(d => d.Id == droneId);
            if (dr == null)
            {
                Console.WriteLine(">>> Błąd: nie ma takiego drona.");
                return false;
            }
            if (dr.Status != DroneStatus.Inactive)
            {
                Console.WriteLine(">>> Dron jest już zajęty.");
                return false;
            }
            dr.Status = DroneStatus.Delivery;
            order.AssignedDrone = dr;
            Console.WriteLine($"Dron '{dr.Name}' #{dr.Id} przypisany do zamówienia #{order.Id}.");
            return true;
        }

        public bool ChangeDroneStatus(int droneId, DroneStatus status)
        {
            var dr = drones.FirstOrDefault(d => d.Id == droneId);
            if (dr == null)
            {
                Console.WriteLine(">>> Brak takiego drona.");
                return false;
            }
            dr.Status = status;
            Console.WriteLine($"Status drona '{dr.Name}' #{dr.Id} zmieniony na {status}.");
            return true;
        }

        public List<Drone> ListDrones(DroneStatus? filter = null)
        {
            return filter.HasValue
                ? drones.Where(d => d.Status == filter.Value).ToList()
                : new List<Drone>(drones);
        }
    }

    class RandomOrderGenerator : IOrderGenerator
    {
        private static readonly string[] Cities = {
            "Warszawa","Kraków","Gdańsk","Wrocław","Poznań","Katowice","Chorzów","Sosnowiec","Sopot","Gdynia"
        };
        private static readonly string[] Streets = {
            "ul.Lipowa","ul.Leśna","ul.Słoneczna","ul.Ogrodowa","ul.Polna","ul.Długa","ul.Szkolna","ul.Jęczmienna","ul.Wiosenna","ul.Chorzowska","ul.Katowicka"
        };
        private static readonly (string first, string last)[] Names = {
            ("Jan","Kowalski"),("Anna","Nowak"),("Piotr","Zieliński"),
            ("Kasia","Wiśniewska"),("Marek","Woźniak"),("Agnieszka","Kaczmarek"),
            ("Tomasz","Mazur"),("Magda","Krawczyk"),("Łukasz","Piotrowski"),
            ("Ewa","Grabowska")
        };
        private readonly Random rand = new Random();

        public Order GenerateOrder()
        {
            var names = Names[rand.Next(Names.Length)];
            var customer = new Customer(names.first, names.last);
            var city = Cities[rand.Next(Cities.Length)];
            var street = Streets[rand.Next(Streets.Length)];
            var order = new Order(customer, city, street);
            Console.WriteLine($"Wygenerowano zamówienie #{order.Id} dla {customer.FirstName} {customer.LastName} w {city}, {street}");
            return order;
        }
    }


    class Program
    {
        static List<Order> orders = new List<Order>();
        static DroneManager droneManager = new DroneManager();
        static RandomOrderGenerator generator = new RandomOrderGenerator();

        static void Main()
        {
            while (true)
            {
                ShowMainMenu();
                var opt = Console.ReadLine();
                switch (opt)
                {
                    case "1": GenerateOrder(); break;
                    case "2": ShowOrderList(); break;
                    case "3": ShowCustomerStats(); break;
                    case "4": ShowDroneMenu(); break;
                    case "5": ShowOrderMenu(); break;
                    case "0": return;
                    default:
                        Console.WriteLine(">>> Nieprawidłowa opcja. Wracam do menu.");
                        break;
                }
            }
        }

        static void ShowMainMenu()
        {
            Console.WriteLine("\n=== DroneDash ===");
            Console.WriteLine("1. Generuj zamówienie");
            Console.WriteLine("2. Lista zamówień");
            Console.WriteLine("3. Klienci + liczba zamówień");
            Console.WriteLine("4. Zarządzanie dronami");
            Console.WriteLine("5. Zarządzanie zamówieniami");
            Console.WriteLine("0. Wyjście");
            Console.Write("Wybierz opcję: ");
        }

        static void GenerateOrder()
        {
            var order = generator.GenerateOrder();
            orders.Add(order);
        }

        static void ShowCustomerStats()
        {
            Console.WriteLine("\n--- Klienci i liczba zamówień ---");

            var counts = new Dictionary<string, int>();

            foreach (var order in orders)
            {
                string fullName = order.Customer.FirstName + " " + order.Customer.LastName;

                if (counts.ContainsKey(fullName))
                {
                    counts[fullName]++;
                }
                else
                {
                    counts[fullName] = 1;
                }
            }

            foreach (var people in counts)
            {
                Console.WriteLine($"{people.Key}: {people.Value}");
            }

            PauseReturn();
        }

        static void ShowDroneMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- Zarządzanie dronami ---");
                Console.WriteLine("1. Dodaj drona");
                Console.WriteLine("2. Przypisz drona do zamówienia");
                Console.WriteLine("3. Zmień status drona");
                Console.WriteLine("4. Lista dronów");
                Console.WriteLine("0. Powrót");
                Console.Write("Opcja: ");
                var o = Console.ReadLine();
                switch (o)
                {
                    case "1": AddDronePrompt(); break;
                    case "2": AssignDronePrompt(); break;
                    case "3": ChangeDroneStatusPrompt(); break;
                    case "4": ListDronesPrompt(); break;
                    case "0": return;
                    default:
                        Console.WriteLine(">>> Nieprawidłowa opcja.");
                        break;
                }
            }
        }

        static void AddDronePrompt()
        {
            Console.Write("Podaj nazwę drona (0 aby anulować): ");
            var name = Console.ReadLine();
            if (name == "0" || string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine(">>> Anulowano dodawanie drona.");
                return;
            }
            droneManager.AddDrone(name);
        }

        static void AssignDronePrompt()
        {
            if (!orders.Any())
            {
                Console.WriteLine("<<< Brak zamówień do przypisania.");
                return;
            }
            Console.Write("Podaj ID zamówienia (0 aby anulować): ");
            if (!int.TryParse(Console.ReadLine(), out var zam_id) || zam_id == 0)
            {
                Console.WriteLine(">>> Anulowano.");
                return;
            }
            var order = orders.FirstOrDefault(x => x.Id == zam_id);
            if (order == null)
            {
                Console.WriteLine(">>> Brak takiego zamówienia.");
                return;
            }
            Console.Write("Podaj ID drona do przypisania (0 aby anulować): ");
            if (!int.TryParse(Console.ReadLine(), out var drone_id) || drone_id == 0)
            {
                Console.WriteLine(">>> Anulowano.");
                return;
            }
            droneManager.AssignDrone(drone_id, order);
        }

        static void ChangeDroneStatusPrompt()
        {
            Console.Write("Podaj ID drona (0 aby anulować): ");
            if (!int.TryParse(Console.ReadLine(), out var drone_id) || drone_id == 0)
            {
                Console.WriteLine(">>> Anulowano.");
                return;
            }
            Console.WriteLine("1. Inactive  2. Active  3. Delivery");
            Console.Write("Wybierz status: ");
            var st = Console.ReadLine();
            DroneStatus d_status;
            switch (st)
            {
                case "1": d_status = DroneStatus.Inactive; break;
                case "2": d_status = DroneStatus.Active; break;
                case "3": d_status = DroneStatus.Delivery; break;
                default:
                    Console.WriteLine(">>> Nieprawidłowy status.");
                    return;
            }
            droneManager.ChangeDroneStatus(drone_id, d_status);
        }

        static void ListDronesPrompt()
        {
            Console.WriteLine("Filtruj: 1.Inactive 2.Active 3.Delivery 4.Wszystkie 0.Powrót");
            Console.Write("Opcja: ");
            var filtr = Console.ReadLine();
            DroneStatus? filter = null;
            switch (filtr)
            {
                case "1": filter = DroneStatus.Inactive; break;
                case "2": filter = DroneStatus.Active; break;
                case "3": filter = DroneStatus.Delivery; break;
                case "4": filter = null; break;
                case "0": return;
                default:
                    Console.WriteLine(">>> Nieprawidłowa opcja.");
                    return;
            }
            var list = droneManager.ListDrones(filter);
            Console.WriteLine("\nID\tNazwa\tStatus");
            foreach (var d in list)
                Console.WriteLine($"{d.Id}\t{d.Name}\t{d.Status}");
            PauseReturn();
        }

        static void ShowOrderMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- Zarządzanie zamówieniami ---");
                Console.WriteLine("1. Zmień status zamówienia");
                Console.WriteLine("2. Lista zamówień");
                Console.WriteLine("0. Powrót");
                Console.Write("Opcja: ");
                var order = Console.ReadLine();
                switch (order)
                {
                    case "1": ChangeOrderStatusPrompt(); break;
                    case "2": ShowOrderList(); break;
                    case "0": return;
                    default:
                        Console.WriteLine(">>> Nieprawidłowa opcja.");
                        break;
                }
            }
        }

        static void ChangeOrderStatusPrompt()
        {
            Console.Write("Podaj ID zamówienia (0 aby anulować): ");
            if (!int.TryParse(Console.ReadLine(), out var order_id) || order_id == 0)
            {
                Console.WriteLine(">>> Anulowano.");
                return;
            }
            var order = orders.FirstOrDefault(o => o.Id == order_id);
            if (order == null)
            {
                Console.WriteLine(">>> Brak takiego zamówienia.");
                return;
            }
            Console.WriteLine("1.New  2.InDelivery  3.Completed  4.Rejected");
            Console.Write("Wybierz status: ");
            var status = Console.ReadLine();
            switch (status)
            {
                case "1": order.Status = OrderStatus.New; break;
                case "2": order.Status = OrderStatus.InDelivery; break;
                case "3": order.Status = OrderStatus.Completed; break;
                case "4": order.Status = OrderStatus.Rejected; break;
                default:
                    Console.WriteLine(">>> Nieprawidłowy status.");
                    return;
            }
            Console.WriteLine($"Status zamówienia #{order.Id} zmieniony na {order.Status}.");

            if ((order.Status == OrderStatus.Completed || order.Status == OrderStatus.Rejected)
                && order.AssignedDrone != null)
            {
                order.AssignedDrone.Status = DroneStatus.Inactive;
                Console.WriteLine($"Dron '{order.AssignedDrone.Name}' #{order.AssignedDrone.Id} zwolniony i ustawiony na Inactive.");
            }
        }

        static void ShowOrderList()
        {
            const int wId = 4;
            const int wName = 20;
            const int wCity = 12;
            const int wStreet = 15;
            const int wStatus = 12;
            const int wDrone = 20;

            Console.WriteLine("\n--- Lista zamówień ---");
            Console.WriteLine(
                "ID".PadRight(wId) +
                "Klient".PadRight(wName) +
                "Miasto".PadRight(wCity) +
                "Ulica".PadRight(wStreet) +
                "Status".PadRight(wStatus) +
                "Dron".PadRight(wDrone)
            );

            foreach (var order in orders)
            {
                var name = $"{order.Customer.FirstName} {order.Customer.LastName}";
                var drone = order.AssignedDrone != null
                    ? $"{order.AssignedDrone.Name} (#{order.AssignedDrone.Id})"
                    : "-";

                Console.WriteLine(
                    order.Id.ToString().PadRight(wId) +
                    name.PadRight(wName) +
                    order.City.PadRight(wCity) +
                    order.Street.PadRight(wStreet) +
                    order.Status.ToString().PadRight(wStatus) +
                    drone.PadRight(wDrone)
                );
            }

            PauseReturn();
        }

        static void PauseReturn()
        {
            Console.WriteLine("\nWciśnij 0 aby powrócić do menu głównego.");
            if (Console.ReadLine() != "0")
                Console.WriteLine(">>> Anulowano, wracam do menu.");
        }
    }
}