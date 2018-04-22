// Buffs are like Skills but for the Buffs list.
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public partial struct Buff {
    // hashcode used to reference the real ItemTemplate (can't link to template
    // directly because synclist only supports simple types). and syncing a
    // string's hashcode instead of the string takes WAY less bandwidth.
    public int hash;

    // dynamic stats (cooldowns etc.)
    public int level;
    public float buffTimeEnd; // server time

    // constructors
    public Buff(BuffSkillTemplate template, int level) {
        hash = template.name.GetStableHashCode();
        this.level = level;
        buffTimeEnd = NetworkTime.time + template.buffTime.Get(level); // start buff immediately
    }

    // template property wrappers for easier access
    public BuffSkillTemplate template { get { return (BuffSkillTemplate)SkillTemplate.dict[hash]; } }
    public string name { get { return template.name; } }
    public Sprite image { get { return template.image; } }
    public float buffTime { get { return template.buffTime.Get(level); } }
    public int buffsHealthMax { get { return template.buffsHealthMax.Get(level); } }
    public int buffsManaMax { get { return template.buffsManaMax.Get(level); } }
    public int buffsDamage { get { return template.buffsDamage.Get(level); } }
    public int buffsDefense { get { return template.buffsDefense.Get(level); } }
    public float buffsBlockChance { get { return template.buffsBlockChance.Get(level); } }
    public float buffsCriticalChance { get { return template.buffsCriticalChance.Get(level); } }
    public float buffsHealthPercentPerSecond { get { return template.buffsHealthPercentPerSecond.Get(level); } }
    public float buffsManaPercentPerSecond { get { return template.buffsManaPercentPerSecond.Get(level); } }
    public int maxLevel { get { return template.maxLevel; } }

    // tooltip - runtime part
    public string ToolTip() {
        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(template.ToolTip(level));

        // addon system hooks
        Utils.InvokeMany(typeof(Buff), this, "ToolTip_", tip);

        return tip.ToString();
    }

    public float BuffTimeRemaining() {
        // how much time remaining until the buff ends? (using server time)
        return NetworkTime.time >= buffTimeEnd ? 0 : buffTimeEnd - NetworkTime.time;
    }
}

public class SyncListBuff : SyncListStruct<Buff> { }
