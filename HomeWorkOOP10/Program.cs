﻿namespace HomeWorkOOP10
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Battlefield battlefield = new();
            battlefield.Run();
        }
    }

    class Battlefield
    {
        private ArmyBuilder _armyBuilder = new ArmyBuilder();

        public void Run()
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
                        StartBattle();
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

        private void StartBattle()
        {
            int maxSoldiers = 10;
            Army firstArmy = _armyBuilder.Build(maxSoldiers, "НАТО");
            Army secondArmy = _armyBuilder.Build(maxSoldiers, "Восточная коалиция");

            Console.WriteLine("Армии стран сформированы, нажмите любую клавишу");
            Console.ReadLine();

            firstArmy.Show();
            Console.WriteLine("");
            secondArmy.Show();

            Console.WriteLine($"{new string('-', 25)}");
            Console.ReadKey();

            while (firstArmy.HasAliveSoldiers && secondArmy.HasAliveSoldiers)
            {
                firstArmy.Attack(secondArmy);
                secondArmy.Attack(firstArmy);
                firstArmy.RemoveDead();
                secondArmy.RemoveDead();
                firstArmy.Heal();
                secondArmy.Heal();

                Console.Clear();

                firstArmy.Show();
                secondArmy.Show();

                Console.ReadKey();
            }

            if (firstArmy.HasAliveSoldiers)
            {
                Console.WriteLine($"Победила армия: {firstArmy.Name}");
            }
            else if(secondArmy.HasAliveSoldiers)
            {
                Console.WriteLine($"Победила армия: {secondArmy.Name}");
            }
            else
            {
                Console.WriteLine("Ничья");
            }

            Console.ReadKey();
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

        public void Attack(Army army)
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

            enemy.Attack(soldier);
        }

        public void Heal()
        {
            foreach(Soldier soldier in _soldiers)
            {
                if(soldier is Medic medic)
                {
                    medic.HealSoldiers(_soldiers);
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

        public virtual void Attack(Soldier soldier)
        {
            soldier.TakeDamage(Damage);
        }

        public void ShowInfo()
        {
            Console.WriteLine($"{Name}, здоровье: {Health}, урон: {Damage}, способность: {Spell}");
        }

        public void TakeDamage(int damage)
        {
            Health -= Damage;
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

        public override void Attack(Soldier soldier)
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

            base.Attack(soldier);
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

        public override void Attack(Soldier soldier)
        {
            Random random = new();
            int criticalDamage = 80;

            if (MortarAttack(random))
            {
                soldier.TakeDamage(criticalDamage);
                Console.WriteLine($"{Name} осуществил минометный обстрел!");
            }

            base.Attack(soldier);
        }

        private bool MortarAttack(Random random)
        {
            int chance = 15;
            int number = random.Next(1, 100);

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

        public override void Attack(Soldier soldier)
        {
            Random random = new();
            int grenadeDamage = 60;

            if (CanThrowGrenade(random))
            {
                soldier.TakeDamage(grenadeDamage);
                Console.WriteLine($"{Name} бросил гранату");
            }

            base.Attack(soldier);
        }

        private bool CanThrowGrenade(Random random)
        {
            int chance = 20;
            int number = random.Next(1, 100);

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

        public override void Attack(Soldier soldier)
        {
            Random random = new();

            if (CounterAttack(random))
            {
                TakeHeal(_selfHealing);
                soldier.TakeDamage(_counterAttackDamage);
                Console.WriteLine($"{Name} восстановил хп и контратаковал в ответ");
            }

            base.Attack(soldier);
        }

        private bool CounterAttack(Random random)
        {
            int chance = 30;
            int number = random.Next(1, 100);

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

        public void HealSoldiers(List<Soldier> soldiers)
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