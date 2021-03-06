using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using POGOProtos.Enums;
using System.Linq;
using System;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Description = "", ItemRequired = Required.DisallowNull)] //Dont set Title
    public class SnipeFilter : BaseConfig
    {
        public SnipeFilter() : base()
        {
            this.Priority = 5;
            Moves = new List<List<PokemonMove>>();
        }

        public SnipeFilter(int snipeMinIV, List<List<PokemonMove>> moves = null) : base()
        {
            this.Operator = "or";
            this.SnipeIV = snipeMinIV;
            this.Moves = moves;
            this.VerifiedOnly = false;
            this.Priority = 5;
        }

        [JsonIgnore]
        [NecrobotConfig(IsPrimaryKey = true,Key = "Enable Snipe", Description = "Enable snipe filter for this, if not set it will apply global setting", Position = 1)]
        [DefaultValue(false)]
        public bool EnableSnipe { get; set; }

        [NecrobotConfig(Key = "Snipe Min IV", Description = "Min Pokemon IV for auto snipe", Position = 2)]
        [DefaultValue(76)]
        [Range(0, 100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public int SnipeIV {get; set;}

        [NecrobotConfig(Key = "Moves", Description = "Defined list of moves that you want snipe", Position = 3)]
        [DefaultValue(null)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public List<List<PokemonMove>> Moves { get; set; }

        [NecrobotConfig(Key = "Operator", Description = "Operator logic check between move and IV", Position = 4)]
        [EnumDataType(typeof(Operator))]
        [DefaultValue("or")]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public string Operator { get; set; }

        [NecrobotConfig(Key = "Verified Only", Description = "Only catch pokemon that has been verified", Position = 5)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public bool VerifiedOnly { get; set; }

        [NecrobotConfig(Key = "Auto Snipe Priority", Description = "Set autosnipe priority", Position = 6)]
        [DefaultValue(5)]
        [Range(1,10)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public int Priority { get; set; }

        [NecrobotConfig(Key = "Auto Snipe Candy", Description = "Set number of candy you want bot snipe for this pokemon", Position = 7)]
        [DefaultValue(2000)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        public int AutoSnipeCandy { get; set; }

        [NecrobotConfig(Key = "Snipe Level", Description = "Min level to snipe , level are using and logic with IV and move and only activate for verify data", Position = 8)]
        [DefaultValue(0)]
        [Range(0,100)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 8)]
        public int Level { get; set; }

        [NecrobotConfig(Key = "AllowMultiAccountSnipe", Description = "Allow bot change account to snipe this pokemon", Position = 9)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 9)]
        public bool AllowMultiAccountSnipe { get; set; }


        internal static Dictionary<PokemonId, SnipeFilter> SniperFilterDefault()
        {
            return new Dictionary<PokemonId, SnipeFilter>
            {
                {PokemonId.Lapras, new SnipeFilter(0, new List<List<PokemonMove>>() { })},
                {PokemonId.Dragonite, new SnipeFilter(0, new List<List<PokemonMove>>() { })},
                {PokemonId.Snorlax, new SnipeFilter(0, new List<List<PokemonMove>>() { })},
                {PokemonId.Dratini, new SnipeFilter(0, new List<List<PokemonMove>>() { })},
                {PokemonId.Rhyhorn, new SnipeFilter(0, new List<List<PokemonMove>>() { })},
                {PokemonId.Abra, new SnipeFilter(0, new List<List<PokemonMove>>() { })}
            };
        }

        public bool IsMatch(double iv, PokemonMove move1, PokemonMove move2, int level, bool verified )
        { 
            var filter = this;
            //if not verified and undetermine move. If not verify, level won't apply
            if (((verified && filter.Level <= level) || !filter.VerifiedOnly) &&
                filter.SnipeIV <= iv &&
                move1 == PokemonMove.MoveUnset && move2 == PokemonMove.MoveUnset &&
                (filter.Moves == null || filter.Moves.Count == 0))
            {
                return true;
            }

            //if not verified and undetermine move. If not verify, level won't apply
            if (((verified && filter.Level <= level) || !verified) &&
                filter.SnipeIV <= iv &&
                move1 == PokemonMove.MoveUnset && move2 == PokemonMove.MoveUnset &&
                (filter.Moves == null || filter.Moves.Count == 0))
            {
                return true;
            }
            //need refactore this to better 
            if (((verified && filter.Level <= level) || !verified) &&
                (string.IsNullOrEmpty(filter.Operator) || filter.Operator == "or") &&
                (filter.SnipeIV <= iv
                 || (filter.Moves != null
                     && filter.Moves.Count > 0
                     && filter.Moves.Any(x => x[0] == move1 && x[1] == move2))
                ))

            {
                return true;
            }

            if (((verified && filter.Level <= level) || !verified) &&
                filter.Operator == "and" &&
                (filter.SnipeIV <= iv
                 && (filter.Moves != null
                     && filter.Moves.Count > 0
                     && filter.Moves.Any(x => x[0] == move1 && x[1] == move2))
                ))
            {
                return true;
            }

            return false;


        }
    }
}