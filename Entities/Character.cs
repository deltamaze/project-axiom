
namespace project_axiom.Entities
{
    public enum CharacterClass
    {
        Brawler,
        Ranger,
        Spellcaster
    }

    public class Character
    {
        public string Name { get; set; } = "";
        public CharacterClass Class { get; set; } = CharacterClass.Brawler;
        
        // Class-specific properties that will be used later
        public int MaxHealth { get; private set; }
        public int MaxResource { get; private set; }
        public string ResourceType { get; private set; }

        public Character()
        {
            SetClassDefaults();
        }

        public Character(string name, CharacterClass characterClass)
        {
            Name = name;
            Class = characterClass;
            SetClassDefaults();
        }

        private void SetClassDefaults()
        {
            switch (Class)
            {
                case CharacterClass.Brawler:
                    MaxHealth = 150;
                    MaxResource = 100;
                    ResourceType = "Rage";
                    break;
                case CharacterClass.Ranger:
                    MaxHealth = 120;
                    MaxResource = 120;
                    ResourceType = "Energy";
                    break;
                case CharacterClass.Spellcaster:
                    MaxHealth = 100;
                    MaxResource = 150;
                    ResourceType = "Mana";
                    break;
            }
        }

        public string GetClassDescription()
        {
            switch (Class)
            {
                case CharacterClass.Brawler:
                    return "Melee combat specialist with heavy armor and high durability.";
                case CharacterClass.Ranger:
                    return "Agile fighter with ranged attacks and nature abilities.";
                case CharacterClass.Spellcaster:
                    return "Master of magical arts with powerful spells and healing.";
                default:
                    return "";
            }
        }
    }
}