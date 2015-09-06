﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace Xania.AspNet.Simulator.Http
{
    internal class HttpContextDecorator: HttpContextBase
    {
        private readonly HttpContextBase _inner;
        private readonly HttpResponseDecorator _response;

        public HttpContextDecorator(HttpContextBase inner)
        {
            _inner = inner;
            _response = new HttpResponseDecorator(inner.Response);
        }

        public override HttpResponseBase Response
        {
            get { return _response; }
        }

        public override HttpRequestBase Request
        {
            get { return _inner.Request; }
        }

        public override IDictionary Items
        {
            get { return _inner.Items; }
        }

        public override HttpSessionStateBase Session
        {
            get { return _inner.Session; }
        }

        public override IPrincipal User
        {
            get { return _inner.User; }
            set { _inner.User = value; }
        }

        public override Cache Cache
        {
            get { return _inner.Cache; }
        }
    }

    internal class HttpResponseDecorator: HttpResponseBase
    {
        private readonly HttpResponseBase _response;
        private TextWriter _output;

        public HttpResponseDecorator(HttpResponseBase response)
        {
            _response = response;
        }

        public override TextWriter Output
        {
            get { return _output ?? _response.Output; }
            set { _output = value; }
        }

        public override void Write(string s)
        {
            Output.Write(s);
        }

        public override string ApplyAppPathModifier(string virtualPath)
        {
            return _response.ApplyAppPathModifier(virtualPath);
        }

        public override HttpCachePolicyBase Cache
        {
            get { return _response.Cache; }
        }
    }
}