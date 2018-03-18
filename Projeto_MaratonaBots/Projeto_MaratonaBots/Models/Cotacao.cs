using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Projeto_MaratonaBots.Models
{
    public class Cotacao
    {
        [JsonProperty("nome")]
        public string Nome { get; set; }

        [JsonProperty("sigla")]
        public string sigla { get; set; }

        [JsonProperty("valor")]
        public float valor { get; set; }

    }
}