﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public interface IHttpIndirectResponse
    {
        bool Include301RedirectPayloadInFragment { get; }
    }
}
