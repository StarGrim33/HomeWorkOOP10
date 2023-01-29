namespace HomeWorkOOP10
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Battlefield battlefield = new();
            Army army = new();
            battlefield.Run(army);
        }
    }

    class Battlefield
    {
        private ArmyBuilder _armyBuilder = new ArmyBuilder();
        private Army? _firstArmy = null;
        private Army? _secondArmy = null;

        public void Run(Army army)
        {
            const string CommandStartBattle = "1";
            const string CommandExit = "2";

            Console.WriteLine($"Приветствуем Вас на поле боя!\n");
            Console.WriteLine($"{CommandStartBattle}-Начать бой");
            Console.WriteLine($"{CommandExit}-Выйти");

            string? userInput = Console.ReadLine();
            bool isProgramOn = true;

            while (isProgramOn)
            {
                switch (userInput)
                {
                    case CommandStartBattle:
                        StartBattle(army);
                        isProgramOn = false;
                        break;

                    case CommandExit:
                        isProgramOn = false;
                        break;

                    default:
                        Console.WriteLine("Введите цифру меню");
                        break;
                }
            }
        }

        private void StartBattle(Army army)
        {
            while(_firstArmy == null && _secondArmy == null)
            {
                if(FormArmy(out army))
                {
                    _firstArmy = army;
                }

                if(FormArmy(out army))
                {
                    _secondArmy = army;
                }
            }

            ShowArmies();
            ToBattle();
            DetermineTheWinner();
        }

        private bool FormArmy(out Army army)
        {
            army = null;
            string? name;

            Console.WriteLine("Введите максимальное количество солдат: ");

            bool isNumber = int.TryParse(Console.ReadLine(), out int maxSoldiers);

            if(isNumber)
            {
                Console.WriteLine("Введите название армии");
                name = Console.ReadLine(); 

                army = _armyBuilder.Build(maxSoldiers, name);
                return true;
            }

            return false;
        }

        private void DetermineTheWinner()
        {
            if (_firstArmy.HasAliveSoldiers)
            {
                Console.WriteLine($"Победила армия: {_firstArmy.Name}");
            }
            else if (_secondArmy.HasAliveSoldiers)
            {
                Console.WriteLine($"Победила армия: {_secondArmy.Name}");
            }
            else
            {
                Console.WriteLine("Ничья");
            }

            Console.ReadKey();
        }

        private void ShowArmies()
        {
            Console.WriteLine("Армии стран сформированы, нажмите любую клавишу");
            Console.ReadLine();

            _firstArmy.Show();
            Console.WriteLine("");
            _secondArmy.Show();

            Console.WriteLine($"{new string('-', 25)}");
            Console.ReadKey();
        }

        private void ToBattle()
        {
            while (_firstArmy.HasAliveSoldiers && _secondArmy.HasAliveSoldiers)
            {
                _firstArmy.ToAttack(_secondArmy);
                _secondArmy.ToAttack(_firstArmy);
                _firstArmy.RemoveDead();
                _secondArmy.RemoveDead();
                _firstArmy.ToHeal();
                _secondArmy.ToHeal();

                Console.Clear();

                _firstArmy.Show();
                _secondArmy.Show();

                Console.ReadKey();
            }
        }
    }

    class Army
    {
        private static Random _random = new Random();
        private readonly List<Soldier> _soldiers;

        public Army(string name, List<Soldier> soldiers)
        {
            Name = name;
            _soldiers = soldiers;
        }

        public Army()
        {
        }

        public string Name { get; }
        public bool HasAliveSoldiers => _soldiers.Count > 0;

        public void Show()
        {
            Console.WriteLine($"Армия:{Name}");
            Console.WriteLine($"{new string('-', 25)}");

            foreach (Soldier soldier in _soldiers)
            {
                Console.WriteLine($"{soldier.Name}, здоровье: {soldier.Health}, урон: {soldier.Damage}");
            }
        }

        public void ToAttack(Army army)
        {
            var soldier = GetRandomSoldier();

            for (int i = 0; i < _soldiers.Count; i++)
            {
                army.TakeDamage(_soldiers[i], soldier);
            }
        }

        private Soldier GetRandomSoldier()
        {
            if (_soldiers.Count == 0)
                return null;

            return _soldiers[_random.Next(_soldiers.Count)];
        }

        public void TakeDamage(Soldier enemy, Soldier soldier)
        {
            if (soldier == null)
                return;

            enemy.ToAttack(soldier);
        }

        public void ToHeal()
        {
            foreach(Soldier soldier in _soldiers)
            {
                if(soldier is Medic medic)
                {
                    medic.ToHealSoldiers(_soldiers);
                }
            }
        }

        public void RemoveDead()
        {
            for(int i = _soldiers.Count - 1; i >= 0; i--)
            {
                if (_soldiers[i].Health < 0)
                {
                    _soldiers.RemoveAt(i);
                }
            }
        }
    }

    class ArmyBuilder
    {
        public Army Build(int soldiersCount, string name)
        {
            List<Soldier> soldiers = new List<Soldier>();

            for (int i = 0; i < soldiersCount; i++)
            {
                soldiers.Add(CreateRandomSoldier());
            }

            return new Army(name, soldiers);
        }

        private List<Soldier> CreateSoldiers()
        {
            return new List<Soldier>()
            {
                new MachineGunner(),
                new MortarMan(),
                new Stormtrooper(),
                new Scout(),
                new Medic()
            };
        }

        private Soldier CreateRandomSoldier()
        {
            Random random = new Random();
            var soldiers = CreateSoldiers();
            int randomIndex = random.Next(soldiers.Count);
            return soldiers[randomIndex];      
        }
    }

    abstract class Soldier
    {
        private int _maxHealth;

        public Soldier(int health)
        {
            _maxHealth = health;
            Health = health;
        }

        public string Name { get; protected set; } = "Стрелок";
        public int Health { get; private set; } = 100;
        public int Damage { get; protected set; } = 25;
        public string Spell { get; protected set; } = " ";

        public virtual void ToAttack(Soldier soldier)
        {
            soldier.TakeDamage(Damage);
        }

        public void ShowInfo()
        {
            Console.WriteLine($"{Name}, здоровье: {Health}, урон: {Damage}, способность: {Spell}");
        }

        public void TakeDamage(int damage)
        {
            Health -= damage;
        }

        public void TakeHeal(int health)
        {
            Health += health;

            if(Health > _maxHealth)
            {
                Health = _maxHealth;
            }
        }
    }

    class MachineGunner : Soldier
    {
        private int _attackCount;

        public MachineGunner() : base(120)
        {
            Name = "Пулеметчик";
            Damage = 30;
            Spell = "Каждый третий выстрел наносит двойной урон противнику";
        }

        public override void ToAttack(Soldier soldier)
        {
            int criticalDamage = 60;
            int criticalAttackNumber = 3;
            _attackCount++;

            if (_attackCount == criticalAttackNumber)
            {
                soldier.TakeDamage(criticalDamage);
                Console.WriteLine($"{Name} нанес критический урон");
                _attackCount = 0;
            }

            base.ToAttack(soldier);
        }
    }

    class MortarMan : Soldier
    {
        private int _chance = 15;

        public MortarMan() : base(60)
        {
            Name = "Минометчик";
            Damage = 40;
            Spell = "С вероятностью" + _chance + "% наносит двойной урон противнику";
        }

        public override void ToAttack(Soldier soldier)
        {
            Random random = new();
            int criticalDamage = 80;

            if (DoMortarAttack(random))
            {
                soldier.TakeDamage(criticalDamage);
                Console.WriteLine($"{Name} осуществил минометный обстрел!");
            }

            base.ToAttack(soldier);
        }

        private bool DoMortarAttack(Random random)
        {
            int minNumber = 1;
            int maxNumber = 100;
            int chance = 15;
            int number = random.Next(minNumber, maxNumber);

            return number < chance;
        }
    }

    class Stormtrooper : Soldier
    {
        private int _chance = 20;

        public Stormtrooper() : base(130)
        {
            Name = "Штурмовик";
            Damage = 25;
            Spell = "С вероятностью" + _chance + "% кидает гранату, которая наносит 60 ед. урона";
        }

        public override void ToAttack(Soldier soldier)
        {
            Random random = new();
            int grenadeDamage = 60;

            if (CanThrowGrenade(random))
            {
                soldier.TakeDamage(grenadeDamage);
                Console.WriteLine($"{Name} бросил гранату");
            }

            base.ToAttack(soldier);
        }

        private bool CanThrowGrenade(Random random)
        {
            int minNumber = 1;
            int maxNumber = 100;
            int chance = 20;
            int number = random.Next(minNumber, maxNumber);

            return number < chance;
        }
    }

    class Scout : Soldier
    {
        private int _chance = 30;
        private int _selfHealing = 40;
        private int _counterAttackDamage = 50;

        public Scout() : base(100)
        {
            Name = "Разведчик";
            Damage = 30;
            Spell = "Шанс" + _chance + "% восстановить" + _selfHealing + "хп и контратаковать в ответ, нанеся" + _counterAttackDamage+ "ед. урона";
        }

        public override void ToAttack(Soldier soldier)
        {
            Random random = new();

            if (ToCounterAttack(random))
            {
                TakeHeal(_selfHealing);
                soldier.TakeDamage(_counterAttackDamage);
                Console.WriteLine($"{Name} восстановил хп и контратаковал в ответ");
            }

            base.ToAttack(soldier);
        }

        private bool ToCounterAttack(Random random)
        {
            int minNumber = 1;
            int maxNumber = 100;
            int chance = 30;
            int number = random.Next(minNumber, maxNumber);

            return number < chance;
        }
    }

    class Medic : Soldier
    {
        public Medic() : base(80)
        {
            Name = "Медик";
            Damage = 20;
        }

        public void ToHealSoldiers(List<Soldier> soldiers)
        {
            int healthRecover = 10;

            foreach(Soldier soldier in soldiers)
            {
                soldier.TakeHeal(healthRecover);
            }

            Console.WriteLine($"Исцелил союзника");
        }
    }
}