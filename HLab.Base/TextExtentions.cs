﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLab.Base
{
    public static class TextExtentions
    {
        public static string ApplySymbols(this string s)
        {
            return s.Replace(">=", "≥")
            .Replace("<=", "≤")
            .Replace("+-", "±")
            .Replace("!=", "≠")
            .Replace("~=", "≈")
            .Replace("/8", "∞")
            .Replace(":alpha","α")
            .Replace(":beta","β")
            .Replace(":gamma ","γ")
            .Replace(":GAMMA ","Γ")
            .Replace(":epsilon ","ε")
            .Replace(":dzeta","ζ")
            .Replace(":eta","η")
            .Replace(":theta","θ")
            .Replace(":THETA","Θ")
            .Replace(":iota","ι")
            .Replace(":kappa","κ")
            .Replace(":lambda","λ")
            .Replace(":LAMBDA","Λ")
            .Replace(":delta","δ")
            .Replace(":DELTA","Δ")
            .Replace(":mu",":μ")
            .Replace(":nu","ν")
            .Replace(":xi","ξ")
            .Replace(":XI","Ξ")
            .Replace(":omicron","ο")
            .Replace(":pi","π")
            .Replace(":PI","Π")
            .Replace(":rho","ρ")
            .Replace(":rho","ρ")
            .Replace(":sigma","σ")
            .Replace(":SIGMA","Σ")
            .Replace(":tau","τ")
            .Replace(":upsilon","υ")
            .Replace(":phi","φ")
            .Replace(":PHI","Φ")
            .Replace(":ki","χ")
            .Replace(":psi","ψ")
            .Replace(":PSI","Ψ")
            .Replace(":omega","ω")
            .Replace(":omega","Ω")
            ;
        }
    }
}
