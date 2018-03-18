using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Projeto_MaratonaBots.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Projeto_MaratonaBots.Dialogs
{
    [Serializable]
    public class LuisDialog : LuisDialog<object>
    {
        public LuisDialog(ILuisService service) : base(service) { }

        /// <summary>
        /// Caso a intenção não seja reconhecida.
        /// </summary>
        [LuisIntent("None")]
        public async Task NoneAsync(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Desculpe, eu não entendi...\n" +
                                    "Lembre-se que sou um bot e meu conhecimento é limitado.");
            context.Done<string>(null);
        }

        /// <summary>
        /// Quando não houve intenção reconhecida.
        /// </summary>
        [LuisIntent("")]
        public async Task IntencaoNaoReconhecida(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Desculpe, mas não entendi o que você quis dizer.\n" +
                                    "Lembre-se que sou um bot e meu conhecimento é limitado.");
            context.Done<string>(null);
        }

        /// <summary>
        /// Intenção sobre.
        /// </summary>
        [LuisIntent("Sobre")]
        public async Task IntencaoSobre(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Eu sou o James Bot, estou sempre aprendendo. Tenha paciência comigo.");
                                    
            context.Done<string>(null);
        }

        /// <summary>
        /// Intenção agradecimento.
        /// </summary>
        [LuisIntent("Agradecimento")]
        public async Task IntencaoAgradecimento(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Muito obrigado, fico feliz, estou aqui para ajudar ....");

            context.Done<string>(null);
        }

        /// <summary>
        /// Intenção de cumprimento.
        /// </summary>
        [LuisIntent("Cumprimento")]
        public async Task IntencaoCumprimento(IDialogContext context, LuisResult result)
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")).TimeOfDay;
            string saudacao;

            if (now < TimeSpan.FromHours(12)) saudacao = "Bom dia";
            else if (now < TimeSpan.FromHours(18)) saudacao = "Boa tarde";
            else saudacao = "Boa noite";

            await context.PostAsync($"{saudacao}! Em que posso ajudar?");
            context.Done<string>(null);
        }

        /// <summary>
        /// Intenção de ajuda.
        /// </summary>
        [LuisIntent("Ajuda")]
        public async Task IntencaoAjuda(IDialogContext context, LuisResult result)
        {
            string ajuda = "";

            ajuda = "Eu posso te ajudar de algumas maneiras, vamos lá.\n" +
                                     "* **Falar que nem gente**\n" +
                                     "* **Realizar cotação de moedas**\n" +
                                     "* **Pesquisar sobre filmes e séries**\n";

            await context.PostAsync(ajuda);
            context.Done<string>(null);
        }

        /// <summary>
        /// Intenção de cotação.
        /// </summary>
        [LuisIntent("Cotacao")]
        public async Task IntencaoCotacao(IDialogContext context, LuisResult result)
        {
            var moedas = result.Entities?.Select(e => e.Entity);
            var filtro = string.Join(",", moedas.ToArray());

            if (filtro.Equals(""))
            {
                string retorno = "";
                retorno = "Entendi que você quer saber sobre o valor das cotações, mas para qual moeda ?";
                await context.PostAsync(retorno);
                context.Done<string>(null);
                return;
            }

            string urlEndpoint = ConfigurationManager.AppSettings["EndpointCotacao"] + $"/{filtro}";

            await context.PostAsync("Aguarde um momento, estou obtendo os valores ...");

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(urlEndpoint);
                if (!response.IsSuccessStatusCode)
                {
                    await context.PostAsync("Ocorreu algum erro... tente novamente mais tarde");
                    return;
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<Cotacao[]>(json);

                    var cotacoes = resultado.Select(c => $"{c.Nome}: {c.valor}");
                    await context.PostAsync($"{string.Join(", ", cotacoes.ToArray())}");
                }

                return;

            }

        }

        /// <summary>
        /// Intenção de filmes e series.
        /// </summary>
        [LuisIntent("Filme_Serie")]
        public async Task IntencaoFilmeSerie(IDialogContext context, LuisResult result)
        {
            var filme_serie = result.Entities?.Select(e => e.Entity);
            string filtro = String.Join(" ", filme_serie.ToArray());
            string retorno = "";

            if (filtro.Equals(""))
            {
                if (result.Query.Contains("filmes") || result.Query.Contains("series") || result.Query.Contains("séries"))
                {
                    retorno = "Entendi que você quer saber sobre filmes ou séries, mas qual seria o filme ou série ?";                    
                }
                else if(result.Query.Contains("filme") || result.Query.Contains("serie") || result.Query.Contains("série"))
                {
                    retorno = "Não encontrei esse filme ou série na nossa base dados ...";
                }
                else
                {
                    retorno = "Fale mais um pouco sobre o filme ou série que procura ...";
                }
                
                await context.PostAsync(retorno);
                context.Done<string>(null);
                return;
            }

            string parameters = $"api_key={ConfigurationManager.AppSettings["ApiKeyTMDB"]}&query={filtro}";  

            string urlEndpoint = ConfigurationManager.AppSettings["EndpointTheMovieDB"] + $"{parameters}";

            await context.PostAsync("Aguarde um momento, estou consultando sobre filmes e séries ...");

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(urlEndpoint);
                if (!response.IsSuccessStatusCode)
                {
                    await context.PostAsync("Ocorreu algum erro... tente novamente mais tarde");
                    return;
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    RootJson resultado = JsonConvert.DeserializeObject<RootJson>(json);

                    if (resultado.TotalResults == 0)
                    {
                        await context.PostAsync("Não encontrei esse filme ou série na nossa base dados ...");                      
                    }
                    else
                    {
                        var filmes_series = resultado.Results;

                        var activity = (Activity)context.Activity;
                        var message = activity.CreateReply();

                        foreach (Results element in filmes_series)
                        {
                            string urlImage = ConfigurationManager.AppSettings["EndpointImagemTMDB"] + element.PosterPath;
                            var heroCard = CreateHeroCard(element.Title, element.ReleaseDate, urlImage);
                            message.Attachments.Add(heroCard.ToAttachment());
                        }

                        await context.PostAsync(message);

                    }


                }

                return;

            }

        }


        private HeroCard CreateHeroCard(string title, string subTitle, string image)
        {
            var heroCard = new HeroCard();
            heroCard.Title = title;
            heroCard.Subtitle = subTitle;
            heroCard.Images = new List<CardImage>
            {
                new CardImage(image, title)
            };

            return heroCard;
        }

    }
}