﻿namespace Common.Contract
{
    public class Permission
    {
        public Function Function { get; set; }
        public Resource Resource { get; set; }
        public Role Role { get; set; }
    }
}