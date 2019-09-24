using System;
using System.Collections.Generic;
using System.Text;

namespace SpamKarmaBase
{
    public class BlackList
    {
        public List<NaggerAddress> NaggerAddresses = new List<NaggerAddress>();
        public List<NaggerDomain> NaggerDomains = new List<NaggerDomain>();
    }
}
