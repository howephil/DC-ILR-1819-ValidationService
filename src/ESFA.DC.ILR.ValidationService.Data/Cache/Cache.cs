﻿using ESFA.DC.ILR.ValidationService.Data.Interface;

namespace ESFA.DC.ILR.ValidationService.Data.Cache
{
    public class Cache<T> : ICache<T>
    {
        public virtual T Item { get; set; }
    }
}
