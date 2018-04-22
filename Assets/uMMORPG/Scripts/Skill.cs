// The Skill struct only contains the dynamic skill properties and a name, so
// that the static properties can be read from the scriptable object. The
// benefits are low bandwidth and easy Player database saving (saves always
// refer to the scriptable skill, so we can change that any time).
//
// Skills have to be structs in order to work with SyncLists.
//
// We implemented the cooldowns in a non-traditional way. Instead of counting
// and increasing the elapsed time since the last cast, we simply set the
// 'end' Time variable to Time.time + cooldown after casting each time. This
// way we don't need an extra Update method that increases the elapsed time for
// each skill all the time.
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public partial struct Skill {
    // hashcode used to reference the real ItemTemplate (can't link to template
    // directly because synclist only supports simple types). and syncing a
    // string's hashcode instead of the string takes WAY less bandwidth.
    public int hash;

    // dynamic stats (cooldowns etc.)
    public int level; // 0 if not learned, >0 if learned
    public float castTimeEnd; // server time
    public float cooldownEnd; // server time

    // constructors
    public Skill(SkillTemplate template) {
        hash = template.name.GetStableHashCode();

        // learned only if learned by default
        level = template.learnDefault ? 1 : 0;

        // ready immediately
        castTimeEnd = cooldownEnd = Time.time;
    }

    // template property wrappers for easier access
    public SkillTemplate template { get { return SkillTemplate.dict[hash]; } }
    public string name { get { return template.name; } }
    public float castTime { get { return template.castTime.Get(level); } }
    public float cooldown { get { return template.cooldown.Get(level); } }
    public float castRange { get { return template.castRange.Get(level); } }
    public int manaCosts { get { return template.manaCosts.Get(level); } }
    public bool followupDefaultAttack { get { return template.followupDefaultAttack; } }
    public Sprite image { get { return template.image; } }
    public bool learnDefault { get { return template.learnDefault; } }
    public bool showCastBar { get { return template.showCastBar; } }
    public bool cancelCastIfTargetDied { get { return template.cancelCastIfTargetDied; } }
    public int maxLevel { get { return template.maxLevel; } }
    public SkillTemplate predecessor { get { return template.predecessor; } }
    public bool requiresWeapon { get { return template.requiresWeapon; } }
    public int upgradeRequiredLevel { get { return template.requiredLevel.Get(level+1); } }
    public long upgradeRequiredSkillExperience { get { return template.requiredSkillExperience.Get(level+1); } }

    // events
    public bool CheckTarget(Entity caster) { return template.CheckTarget(caster); }
    public bool CheckDistance(Entity caster, out Vector3 destination) { return template.CheckDistance(caster, level, out destination); }
    public void Apply(Entity caster) { template.Apply(caster, level); }

    // tooltip - dynamic part
    public string ToolTip(bool showRequirements = false) {
        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(template.ToolTip(level, showRequirements));

        // addon system hooks
        Utils.InvokeMany(typeof(Skill), this, "ToolTip_", tip);

        // only show upgrade if learned and not max level yet
        if (0 < level && level < maxLevel) {
            tip.Append("\n<i>Upgrade:</i>\n" +
                       "<i>  Required Level: " + upgradeRequiredLevel + "</i>\n" +
                       "<i>  Required Skill Exp.: " + upgradeRequiredSkillExperience + "</i>\n");
        }

        return tip.ToString();
    }

    public float CastTimeRemaining() {
        // how much time remaining until the casttime ends? (using server time)
        return NetworkTime.time >= castTimeEnd ? 0 : castTimeEnd - NetworkTime.time;
    }

    public bool IsCasting() {
        // we are casting a skill if the casttime remaining is > 0
        return CastTimeRemaining() > 0;
    }

    public float CooldownRemaining() {
        // how much time remaining until the cooldown ends? (using server time)
        return NetworkTime.time >= cooldownEnd ? 0 : cooldownEnd - NetworkTime.time;
    }

    public bool IsReady() {
        return CooldownRemaining() == 0;
    }
}

public class SyncListSkill : SyncListStruct<Skill> { }
