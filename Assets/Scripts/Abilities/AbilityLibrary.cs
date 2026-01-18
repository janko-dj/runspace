namespace GameCore
{
    public static class AbilityLibrary
    {
        public static PlayerAbility Create(string abilityId)
        {
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                return null;
            }

            switch (abilityId)
            {
                case "gunner_burst":
                    return new GunnerBurstAbility();
                case "gunner_overdrive":
                    return new GunnerOverdriveAbility();
                case "short_ability":
                    return new ShortCooldownAbility();
                case "long_ability":
                    return new LongCooldownAbility();
                default:
                    return null;
            }
        }
    }
}
