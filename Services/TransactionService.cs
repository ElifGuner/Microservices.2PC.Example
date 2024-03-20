using Coordinator.Models;
using Coordinator.Models.Contexts;
using Coordinator.Services.Abstractions;
using System;
using Microsoft.EntityFrameworkCore;

namespace Coordinator.Services
{
    public class TransactionService : ITransactionService
    {
        TwoPhaseCommitContext _context;

        //IHttpClientFactory _httpClientFactory;
        HttpClient _orderHttpClient;
        HttpClient _stockHttpClient;
        HttpClient _paymentHttpClient;

        int transactionId = 0;
        int stateId = 0;
        public TransactionService(IHttpClientFactory httpClientFactory, TwoPhaseCommitContext context)
        {
            _context = context;

            _orderHttpClient = httpClientFactory.CreateClient("OrderAPI");
            _stockHttpClient = httpClientFactory.CreateClient("StockAPI");
            _paymentHttpClient = httpClientFactory.CreateClient("PaymentAPI");
        }

        public async Task<int> CreateTransactionAsync()
        {
            ++transactionId;
            var nodes = await _context.Nodes.ToListAsync();

            //foreach (var node in nodes)
            //    nodes.ForEach(node => node.NodeState = new List<NodeState>()
            //    {
            //       // NodeState ns = new NodeState()
            //       new()
            //        {
            //           // Id = ++stateId,
            //            TransactionId = transactionId,
            //            IsReady = enums.ReadyType.Pending,
            //            TransactionState = enums.TransactionState.Pending
            //        }
            //});

            nodes.ForEach(node => //node.NodeState
            {
                 NodeState ns = new NodeState()
                //new NodeState()
                {
                    //Id = ++stateId,
                    TransactionId = transactionId,
                    IsReady = enums.ReadyType.Pending,
                    TransactionState = enums.TransactionState.Pending,
                    Node = node
                };
                _context.NodeStates.Add(ns);
              //  _context.SaveChanges();
            });

            _context.SaveChanges();
            return transactionId;
        }

        public async Task PrepareServicesAsync(int transactionId)
        {
            var transactionNodes = await _context.NodeStates.Include(ns => ns.Node).Where(ns => ns.TransactionId == transactionId).ToListAsync();
            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    var response = await (transactionNode.Node.Name switch
                    {
                        "Order.API" => _orderHttpClient.GetAsync("ready"),
                        "Stock.API" => _stockHttpClient.GetAsync("ready"),
                        "Payment.API" => _paymentHttpClient.GetAsync("ready")
                    });

                    var result = bool.Parse(await response.Content.ReadAsStringAsync());
                    transactionNode.IsReady = result ? enums.ReadyType.Ready : enums.ReadyType.UnReady;
                }
                catch
                {
                    transactionNode.IsReady = enums.ReadyType.UnReady;
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CheckReadyServicesAsync(int transactionId) 
        {
            //return await (_context.NodeStates
            //           .Where(ns => ns.TransactionId == transactionId).ToListAsync()).TrueForAll(ns => ns.IsReady == enums.ReadyType.Ready);

            List<NodeState> li = await (_context.NodeStates
                           .Where(ns => ns.TransactionId == transactionId).ToListAsync());
            return li.TrueForAll(ns => ns.IsReady == enums.ReadyType.Ready);
        }

        public async Task CommitAsync(int transactionId)
        {
            var transactionNodes = await _context.NodeStates.Include(ns => ns.Node).Where(ns => ns.TransactionId == transactionId).ToListAsync();
            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    var response = await(transactionNode.Node.Name switch
                    {
                        "Order.API" => _orderHttpClient.GetAsync("commit"),
                        "Stock.API" => _stockHttpClient.GetAsync("commit"),
                        "Payment.API" => _paymentHttpClient.GetAsync("commit")
                    });

                    var result = bool.Parse(await response.Content.ReadAsStringAsync());
                    transactionNode.TransactionState = result ? enums.TransactionState.Done : enums.TransactionState.Abort;
                }
                catch
                {
                    transactionNode.TransactionState = enums.TransactionState.Abort;
                }
            }
            await _context.SaveChangesAsync();
        }
        public async Task<bool> CheckTransactionStateServicesAsync(int transactionId)
        {
            List <NodeState> nodeStatesList = await (_context.NodeStates.Where(ns => ns.TransactionId == transactionId).ToListAsync());
            return nodeStatesList.TrueForAll(ns => ns.TransactionState == enums.TransactionState.Done);
        }

        public async Task RollbackAsync(int transactionId)
        {
            var transactionNodes = await _context.NodeStates.Include(ns => ns.Node).Where(ns => ns.TransactionId == transactionId).ToListAsync();
            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    if (transactionNode.TransactionState == enums.TransactionState.Done)
                    {
                        _ = await(transactionNode.Node.Name switch
                        {
                            "Order.API" => _orderHttpClient.GetAsync("rollback"),
                            "Stock.API" => _stockHttpClient.GetAsync("rollback"),
                            "Payment.API" => _paymentHttpClient.GetAsync("rollback")
                        });

                        transactionNode.TransactionState = enums.TransactionState.Abort;
                    }
                }
                catch
                {
                    transactionNode.TransactionState = enums.TransactionState.Abort;
                }
            }
            await _context.SaveChangesAsync();
        }
    }
    
}
