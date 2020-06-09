using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoConsumirRedis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PessoaController : ControllerBase
    {
        private readonly IDistributedCache _cache;
        private const string CHAVE_PESSOAS = "CHAVE_PESSOAS";

        public PessoaController(IDistributedCache cache)
        {
            _cache = cache;
        }

        // GET: api/Pessoa
        [HttpGet("")]
        public async Task<IActionResult> GetPessoa()
        {
            try
            {
                //Farei o código todo aqui para fins didáticos
                var pessoaList = await ObterPessoas();

                return Ok(pessoaList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // POST: api/Pessoa
        [HttpPost("")]
        public async Task<IActionResult> PostPessoa([FromBody] Pessoa pessoa)
        {
            var cacheSettings = new DistributedCacheEntryOptions();
            cacheSettings.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

            var pessoaInput = JsonConvert.SerializeObject(pessoa);

            await _cache.SetStringAsync(CHAVE_PESSOAS, pessoaInput, cacheSettings);

            return Ok(pessoaInput);
        }

        private async Task<IEnumerable<Pessoa>> ObterPessoas()
        {
            var dataCache = await _cache.GetStringAsync(CHAVE_PESSOAS);


            var cacheSettings = new DistributedCacheEntryOptions();
            cacheSettings.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

            if (string.IsNullOrWhiteSpace(dataCache))
            {
                var pessoasFromDatabase = MockPessoas();

                var itemsJson = JsonConvert.SerializeObject(pessoasFromDatabase);

                await _cache.SetStringAsync(CHAVE_PESSOAS, itemsJson, cacheSettings);

                return await Task.FromResult(pessoasFromDatabase);
            }

            var pessoasFromCache = JsonConvert.DeserializeObject<IEnumerable<Pessoa>>(dataCache);

            return await Task.FromResult(pessoasFromCache);
        }

        private IEnumerable<Pessoa> MockPessoas()
        {
            var count = 1;

            var list = new Faker<Pessoa>()
                .RuleFor(p => p.Id, p => count++)
                .RuleFor(p => p.Nome, p => p.Person.FirstName)
                 .RuleFor(p => p.Sobrenome, p => p.Person.LastName)
                .RuleFor(p => p.Idade, count++ + 20)
                .GenerateLazy(100);

            return list;
        }


    }
}