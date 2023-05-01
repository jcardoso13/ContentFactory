﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TransformData.ContentFactory
{
    public interface IContentWriter
    {
        StringContent CreateResponse(string output);
    }
}
