using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

using TuTa.Wms.BarcodeLists;
using TuTa.Wms.BarcodeLists.ValueObjects;
using TuTa.Wms.ChkResultLists;
using TuTa.Wms.Moves;
using TuTa.Wms.Moves.Aggregates;
using TuTa.Wms.Stocks;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.Erp
{
    public class ErpMoveSyncJob : IHostedService,IDisposable
    {
        private readonly IErpMoveRepository _erpMoveRepository;
        private readonly IMoveRepository _moveRepository;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<ErpMoveSyncJob> _logger;

        public ErpMoveSyncJob(
            UnitOfWorkManager unitOfWorkManager
            , ILogger<ErpMoveSyncJob> logger
            , IErpMoveRepository erpMoveRepository
            , IMoveRepository moveRepository
            )
        {
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
            _erpMoveRepository = erpMoveRepository;
            _moveRepository = moveRepository;
        }

        public void Dispose()
        {

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            int sleepTime = 60000;
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(sleepTime).ConfigureAwait(false); //间隔一定时间

                    using (var uow = _unitOfWorkManager.Begin())
                    {
                        try
                        {

                            var erpMoves = await _erpMoveRepository.GetAllUnReceivedMovesAsync(true, cancellationToken);
                            if (erpMoves == null || erpMoves.Count == 0)
                            {
                                //await Task.Delay(sleepTime).ConfigureAwait(false); //间隔一定时间更新一次
                                continue;
                            }

                            foreach (var erpMove in erpMoves)
                            {

                                if (erpMove.IFDELETE != true) //不删，则增加或修改
                                {
                                    var move = await _moveRepository.FindByMoveCodeAsync(erpMove.ZCDBD_ID).ConfigureAwait(false);
                                    if (move == null) //本地没有该检验后数据，添加
                                    {
                                        move = new Move(
                                            erpMove.ZCDBD_ID,
                                            erpMove.ZCDBD_DATE,
                                            erpMove.PRDT_PH,
                                            erpMove.PRDT_ID,
                                            erpMove.PRDT_NAME,
                                            erpMove.PRDT_SPEC,
                                            erpMove.PRDT_UNIT,
                                            erpMove.ZCDB_NUM
                                            );
                                        await _moveRepository.InsertAsync(move);
                                    }
                                    else //已经存在该入库单，修改
                                    {
                                        if (move.MoveCount != 0) //只有在入库前才能修改
                                        {
                                            move.ModifyMove(
                                            erpMove.ZCDBD_DATE,
                                            erpMove.PRDT_PH,
                                            erpMove.PRDT_ID,
                                            erpMove.PRDT_NAME,
                                            erpMove.PRDT_SPEC,
                                            erpMove.PRDT_UNIT,
                                            erpMove.ZCDB_NUM);
                                            await _moveRepository.UpdateAsync(move);
                                        }
                                    }

                                    erpMove.SetIsReceived();
                                    await _erpMoveRepository.UpdateAsync(erpMove);
                                }
                                else //删
                                {
                                    var move = await _moveRepository.FindByMoveCodeAsync(erpMove.ZCDBD_ID).ConfigureAwait(false);
                                    if (move != null) //本地已有该入库单，删除
                                        await _moveRepository.DeleteAsync(move).ConfigureAwait(false);

                                    erpMove.SetIsReceived();
                                    await _erpMoveRepository.UpdateAsync(erpMove);
                                }

                                await uow.SaveChangesAsync();

                                await Task.Delay(5).ConfigureAwait(false);
                            }
                            
                            await uow.CompleteAsync();
                        }
                        catch (Exception ex)
                        {
                            await uow.RollbackAsync();
                            _logger.Error(ex.Message);
                        }
                    }

                    //await Task.Delay(sleepTime).ConfigureAwait(false); //1分钟更新一次
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
