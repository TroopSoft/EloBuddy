using System;
using System.Linq;
using SharpDX;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;

namespace EloBuddy_Veigar
{
    internal class Program
    {
        public static Spell.Skillshot Q;

        public static Spell.Skillshot W;

        public static Spell.Skillshot E;

        private static readonly AIHeroClient player = ObjectManager.Player;

        public Vector3 EPos { get; set; }

        public static Spell.Targeted R;

        public static Menu Menu,
                           SkillsMenu,
                           FarmingMenu,
                           MiscsMenu,
                           DrawingsMenu,
                           HarassMenu,
                           ComboMenu,
                           KsMenu,
                           DrawMenu,
                           UpdateMenu;

        private static void Main()
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static AIHeroClient _Player
        {
            get
            {
                return Player.Instance;
            }
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Veigar") return;


            //ITEMS CAN BE SET IN HERE

            Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Linear, 250, 950, 75);
            W = new Spell.Skillshot(SpellSlot.W, 950, SkillShotType.Linear, 400, 1060, 312);
            E = new Spell.Skillshot(SpellSlot.E, 1050, SkillShotType.Linear, 400, 1050, 210);
            R = new Spell.Targeted(SpellSlot.R, 650);

            Menu = MainMenu.AddMenu("Soft Veigar", "softveigar");

            ComboMenu = Menu.AddSubMenu("Combo Settings", "ComboSettings");
            ComboMenu.AddLabel("Combo Settings");
            ComboMenu.Add("QCom", new CheckBox("Use Q"));
            ComboMenu.Add("WCom", new CheckBox("Use W"));
            ComboMenu.Add("ECom", new CheckBox("Use E"));
            ComboMenu.Add("RCom", new CheckBox("Use R"));


            FarmingMenu = Menu.AddSubMenu("Lane Clear", "FarmSettings");

            FarmingMenu.AddLabel("Lane Clear");
            FarmingMenu.Add("QLaneClear", new CheckBox("Use Q to LaneClear"));
            FarmingMenu.Add("QlaneclearManaPercent", new Slider("Mana < %", 45));
            FarmingMenu.Add("WLaneClear", new CheckBox("Use W to LaneClear"));
            FarmingMenu.Add("WlaneclearManaPercent", new Slider("Mana < %", 45));


            FarmingMenu.AddLabel("Last Hit Settings");
            FarmingMenu.Add("Qlasthit", new CheckBox("Use Q LastHit"));
            FarmingMenu.Add("QlasthitManapercent", new Slider("Mana < %", 45));


            MiscsMenu = Menu.AddSubMenu("Misc", "Misc");

            MiscsMenu.AddLabel("Auto");
            MiscsMenu.Add("Auto Ignite", new CheckBox("Auto Ignite"));
            MiscsMenu.Add("autoRenemyHP", new Slider("Enemy HP < %", 10));
            MiscsMenu.Add("InterruptSpellsE", new CheckBox("Use E Interrupt Spells"));

            KsMenu = Menu.AddSubMenu("Ks", "KS Menu");

            KsMenu.AddLabel("KillSteal");
            KsMenu.Add("Qks", new CheckBox("Use Q to KillSteal"));
            KsMenu.Add("Rks", new CheckBox("Use R to KillSteal"));

            HarassMenu = Menu.AddSubMenu("Harass", "Harass");
            HarassMenu.Add("HarassQ", new CheckBox("Use Q to harass"));
            HarassMenu.Add("HarassW", new CheckBox("Use W to harass"));

            DrawingsMenu = Menu.AddSubMenu("Drawings", "Drawings");
            DrawingsMenu.Add("drawAARange", new CheckBox("Draw AA Range"));
            DrawingsMenu.Add("drawQ", new CheckBox("Draw Q"));
            DrawingsMenu.Add("drawW", new CheckBox("Draw W "));
            DrawingsMenu.Add("drawE", new CheckBox("Draw E"));
            DrawingsMenu.Add("drawR", new CheckBox("Draw R"));

            UpdateMenu = Menu.AddSubMenu("Last Update Logs", "Updates");

            UpdateMenu.Add("release", new CheckBox(" 06/03/2016 RELEASE "));
            UpdateMenu.Add("release", new CheckBox(" 07/03/2016 FIXED COMBO // Added E Interrupt"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnInterruptableSpell += Interrupter2_OnInterruptableTarget;

            Chat.Print("Soft Veigar by TroopSoft Loaded", Color.Aqua)
                ;
            Chat.Print("If any Bug does exist , message TroopSoft on EloBuddy!", Color.CadetBlue);
            Chat.Print("Dominate this game!!", Color.Green);
            Chat.Print("HAVE FUN!", Color.LightSalmon);

        }

        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            Killsteal();

        }


        private static void Interrupter2_OnInterruptableTarget(
            Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {

            if (Menu.Get<CheckBox>("Misc").CurrentValue)
            {
                return;
            }

            if (!sender.IsValidTarget() || !sender.IsEnemy || sender.IsAlly || sender.IsMe || sender == null)
            {
                return;
            }

            if (Menu.Get<CheckBox>("InterruptSpellsE").CurrentValue)
            {
                if (E.IsReady() && sender.IsValidTarget(E.Range))
                {
                    E.Cast(sender);
                    return;
                }
            }
        }


        private static void Harass()
        {
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                var pred = Q.GetPrediction(target);
                if (target != null && pred.HitChance >= HitChance.Medium
                    && pred.GetCollisionObjects<Obj_AI_Base>().Count() > 1
                    && pred.GetCollisionObjects<Obj_AI_Base>()[0].NetworkId == target.NetworkId)
                    if (HarassMenu["harassQ"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(Q.Range))
                    {
                        if (player.Distance(target.ServerPosition) >= 700)
                        {
                            Q.Cast(target);
                        }
                    }
            }
        }

        private static void Killsteal()
        {
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                var useQ = KsMenu["Qks"].Cast<CheckBox>().CurrentValue;
                var useR = KsMenu["Rks"].Cast<CheckBox>().CurrentValue;

                if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range) && !target.IsZombie
                    && target.Health <= _Player.GetSpellDamage(target, SpellSlot.Q))
                {
                    Q.Cast(target);
                }
                if (R.IsReady() && useR && target.IsValidTarget(R.Range) && !target.IsZombie
                    && target.Health <= _Player.GetSpellDamage(target, SpellSlot.R))
                {
                    if (!target.HasBuffOfType(BuffType.Invulnerability) && !target.HasBuffOfType(BuffType.SpellShield)
                        && !target.HasBuff("kindredrnodeathbuff") //Kindred Ult
                        && !target.HasBuff("FioraW") //Fiora W
                        && !target.HasBuff("JudicatorIntervention") //Kayle R
                        && !target.HasBuff("UndyingRage") //Trynd R
                        && !target.HasBuff("BardRStasis") //Bard R
                        && !target.HasBuff("ChronoShift") //Zilean R
                        )
                    {
                        R.Cast(target);
                    }
                }
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            var useQ = ComboMenu["QCom"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["WCom"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ECom"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["RCom"].Cast<CheckBox>().CurrentValue;
            var Epredict = E.GetPrediction(target);
            var hithere = Epredict.CastPosition.Extend(ObjectManager.Player.Position, -200);

            if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.High && !target.IsDead
                && !target.IsZombie)
            {
                Q.Cast(target);
            }
            if (useE && E.IsReady() && E.GetPrediction(target).HitChance >= HitChance.Medium && !target.IsDead
                && !target.IsZombie)
            {
                E.Cast((Vector3)hithere);
            }
            if (useW && W.IsReady() && W.GetPrediction(target).HitChance >= HitChance.High && !target.IsDead
                && !target.IsZombie)
            {
                W.Cast(target);
            }
            if (useR && R.IsReady())
            {
                if (target.IsValidTarget(R.Range) && !target.IsZombie)
                {
                    if (!target.HasBuffOfType(BuffType.Invulnerability) && !target.HasBuffOfType(BuffType.SpellShield)
                        && !target.HasBuff("kindredrnodeathbuff") //Kindred Ult
                        && !target.HasBuff("FioraW") //Fiora W
                        && !target.HasBuff("JudicatorIntervention") //Kayle R
                        && !target.HasBuff("UndyingRage") //Trynd R
                        && !target.HasBuff("BardRStasis") //Bard R
                        && !target.HasBuff("ChronoShift") //Zilean R
                        )
                    {
                        R.Cast(target);
                    }
                }
            }
        }

        private static void LaneClear()
        {
            var Qmana = FarmingMenu["QlaneclearManaPercent"].Cast<Slider>().CurrentValue;
            var Wmana = FarmingMenu["WlaneclearManaPercent"].Cast<Slider>().CurrentValue;
                var useQ = FarmingMenu["QLaneClear"].Cast<CheckBox>().CurrentValue;
                var useW = FarmingMenu["WLaneClear"].Cast<CheckBox>().CurrentValue;
                var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
                foreach (var minion in minions)
                {
                    if (useQ && Q.IsReady() && Player.Instance.ManaPercent > Qmana && minion.IsValidTarget(Q.Range) && minions.Count() >= 2)
                {
                        Q.Cast(minion);
                    }
                    if (useW && W.IsReady() && Player.Instance.ManaPercent > Wmana && minion.IsValidTarget(W.Range) && minions.Count() >= 3)
                    {
                        W.Cast(minion);
                    }
                }
        }

        private static void LastHit()
        {
            var useQ = FarmingMenu["Qlasthit"].Cast<CheckBox>().CurrentValue;
            var minions =
                ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range)
                    && minion.Health < _Player.GetSpellDamage(minion, SpellSlot.Q))
                    if (Orbwalker.IsAutoAttacking)
                        return;
                {
                    Q.Cast(minion);
                }
            }
        }

private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawingsMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Aqua, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (DrawingsMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.DarkSeaGreen, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (DrawingsMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
            if (DrawingsMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.LightSalmon, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}
