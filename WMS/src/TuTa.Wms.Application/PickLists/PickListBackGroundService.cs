using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Materials;
using TuTa.Wms.PickLists;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.Erp
{
    public class PickListBackGroundService : IHostedService, IDisposable
    {
        private readonly IPickListRepository _pickListRepository;
        private readonly PickListManager _pickListManager;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<PickListBackGroundService> _logger;

        public PickListBackGroundService(
            IPickListRepository pickListRepository,
            PickListManager pickListManager,
            UnitOfWorkManager unitOfWorkManager,
            ILogger<PickListBackGroundService> logger)
        {
            _pickListRepository = pickListRepository;
            _pickListManager = pickListManager;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        public void Dispose()
        {

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(5000).ConfigureAwait(false); //1分钟更新一次

                    using (var uow = _unitOfWorkManager.Begin())
                    {

                        //try
                        //{
                        //    var pickLists = await _pickListRepository.GetAllPickListsAsync().ConfigureAwait(false);
                        //    if (pickLists == null || pickLists.Count == 0)
                        //        continue;

                        //    foreach(var pickList in pickLists)
                        //    {
                        //        _pickListManager.CleanPickListStocksWhichAreTimeOver(pickList);
                        //        await uow.SaveChangesAsync().ConfigureAwait(false);
                        //    }

                        //    await uow.CompleteAsync();
                        //}
                        //catch (Exception ex)
                        //{
                        //    await uow.RollbackAsync().ConfigureAwait(false);
                        //    _logger.Error(ex.Message);
                        //}

                        using (HttpClient client = new HttpClient())
                        {
                            string apiUrl = $"http://192.168.0.4:327/ecs/LabelOver";
                            HttpResponseMessage response = await client.GetAsync(apiUrl);

                            apiUrl = $"http://192.168.0.4:327/ecs/BandingOver";
                            await client.GetAsync(apiUrl);
                        }
                    }
                }

            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
