using FinancePro.Application.Accounting;
using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Core.Enums;
using FinancePro.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Data.Accounting;

public sealed class AccountingGateway : IAccountingGateway
{
    private readonly FinanceProDbContext _db;
    public AccountingGateway(FinanceProDbContext db) => _db = db;

    public async Task<IReadOnlyList<PlanoContaDto>> ListAccountsAsync(int empresaId, string? pesquisa, CancellationToken ct = default)
    {
        var q = _db.PlanoContas.AsNoTracking().Where(x => x.EmpresaId == empresaId);
        if (!string.IsNullOrWhiteSpace(pesquisa)) q = q.Where(x => x.Codigo.Contains(pesquisa) || x.Nome.Contains(pesquisa));
        return await q.OrderBy(x => x.Codigo).Select(x => new PlanoContaDto { Id=x.Id, Codigo=x.Codigo, Nome=x.Nome, Tipo=x.Tipo, Natureza=x.Natureza, AceitaLancamentos=x.AceitaLancamentos, CentroCustoObrigatorio=x.CentroCustoObrigatorio, ContaPaiId=x.ContaPaiId, Ativo=x.Ativo }).ToListAsync(ct);
    }

    public Task<bool> AccountCodeExistsAsync(int empresaId, string codigo, int ignoreId, CancellationToken ct = default) => _db.PlanoContas.AnyAsync(x => x.EmpresaId == empresaId && x.Codigo == codigo && x.Id != ignoreId, ct);

    public async Task<int> SaveAccountAsync(AccountSaveRequest r, CancellationToken ct = default)
    {
        PlanoContas e;
        if (r.Id == 0) { e = new PlanoContas { EmpresaId = r.EmpresaId }; _db.PlanoContas.Add(e); }
        else { e = await _db.PlanoContas.SingleAsync(x => x.Id == r.Id && x.EmpresaId == r.EmpresaId, ct); e.DataAtualizacao=DateTime.UtcNow; }
        e.Codigo=r.Codigo; e.Nome=r.Nome; e.Tipo=r.Tipo; e.Natureza=r.Natureza; e.AceitaLancamentos=r.AceitaLancamentos; e.CentroCustoObrigatorio=r.CentroCustoObrigatorio; e.ContaPaiId=r.ContaPaiId; e.Ativo=r.Ativo;
        await _db.SaveChangesAsync(ct); return e.Id;
    }

    public async Task SetAccountActiveAsync(int id, bool ativo, CancellationToken ct = default) { var e=await _db.PlanoContas.SingleAsync(x=>x.Id==id,ct); e.Ativo=ativo; e.DataAtualizacao=DateTime.UtcNow; await _db.SaveChangesAsync(ct); }

    public async Task<IReadOnlyList<LancamentoContabilDto>> ListEntriesAsync(int empresaId, CancellationToken ct = default) => await _db.LancamentosContabeis.AsNoTracking().Where(x=>x.EmpresaId==empresaId).OrderByDescending(x=>x.DataLancamento).ThenByDescending(x=>x.Id).Select(x=>new LancamentoContabilDto { Id=x.Id, Numero=x.Numero, DataLancamento=x.DataLancamento, Descricao=x.Descricao, Estado=x.Estado, TotalDebito=x.Linhas.Sum(l=>l.Debito), TotalCredito=x.Linhas.Sum(l=>l.Credito) }).ToListAsync(ct);

    public async Task<int> SaveEntryAsync(JournalEntryRequest r, CancellationToken ct = default)
    {
        var validAccounts=await _db.PlanoContas.Where(x=>x.EmpresaId==r.EmpresaId && x.Ativo && x.AceitaLancamentos).Select(x=>x.Id).ToListAsync(ct);
        if (r.Linhas.Any(x=>!validAccounts.Contains(x.PlanoContasId))) throw new InvalidOperationException("Uma ou mais contas não aceitam lançamentos.");
        var next=(await _db.LancamentosContabeis.CountAsync(x=>x.EmpresaId==r.EmpresaId,ct))+1;
        var e=new LancamentoContabil { EmpresaId=r.EmpresaId, DataLancamento=r.DataLancamento, Numero=$"LC-{r.DataLancamento:yyyy}-{next:000000}", Descricao=r.Descricao, DocumentoReferencia=r.DocumentoReferencia, OrigemModulo=r.OrigemModulo, UtilizadorId=r.UtilizadorId };
        foreach(var l in r.Linhas) e.Linhas.Add(new LancamentoContabilLinha { PlanoContasId=l.PlanoContasId, Descricao=l.Descricao, Debito=l.Debito, Credito=l.Credito, CentroCusto=l.CentroCusto });
        _db.LancamentosContabeis.Add(e); await _db.SaveChangesAsync(ct); return e.Id;
    }

    public async Task PostEntryAsync(int id, CancellationToken ct = default)
    {
        var e=await _db.LancamentosContabeis.Include(x=>x.Linhas).SingleAsync(x=>x.Id==id,ct);
        if(e.Estado!=EstadoLancamentoContabil.Rascunho) throw new InvalidOperationException("Apenas lançamentos em rascunho podem ser contabilizados.");
        if(e.Linhas.Sum(x=>x.Debito)!=e.Linhas.Sum(x=>x.Credito)) throw new InvalidOperationException("O lançamento não está balanceado.");
        e.Estado=EstadoLancamentoContabil.Contabilizado; e.DataContabilizacao=DateTime.UtcNow; e.DataAtualizacao=DateTime.UtcNow; await _db.SaveChangesAsync(ct);
    }
}
