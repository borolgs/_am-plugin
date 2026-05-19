using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AlfaMap.DataSync {
    public class DefaultQueryHandler : DelegatingHandler {
        private readonly IReadOnlyDictionary<string, string> defaults;

        public DefaultQueryHandler(IReadOnlyDictionary<string, string> defaults) {
            this.defaults = defaults;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) {
            var uriBuilder = new UriBuilder(request.RequestUri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            foreach (var kvp in defaults) {
                if (string.IsNullOrEmpty(query[kvp.Key])) {
                    query[kvp.Key] = kvp.Value;
                }
            }

            uriBuilder.Query = query.ToString();
            request.RequestUri = uriBuilder.Uri;

            return base.SendAsync(request, cancellationToken);
        }
    }
}
