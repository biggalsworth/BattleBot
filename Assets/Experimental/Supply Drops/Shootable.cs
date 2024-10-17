using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Shootable { 
    public abstract void TakeHit(uint damage, BotCompetitor shooter);
}
