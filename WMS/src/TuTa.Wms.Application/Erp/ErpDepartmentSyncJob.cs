using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Departments;
using TuTa.Wms.Departments.Aggregates;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.Erp
{
    public class ErpDepartmentSyncJob : IHostedService, IDisposable
    {
        private readonly IErpDepartmentRepository _erpDepartmentRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly DepartmentManager _departmentManager;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<ErpDepartmentSyncJob> _logger;

        public ErpDepartmentSyncJob(
            IErpDepartmentRepository erpDepartmentRepository,
            IDepartmentRepository departmentRepository,
            DepartmentManager departmentManager,
            UnitOfWorkManager unitOfWorkManager,
            ILogger<ErpDepartmentSyncJob> logger)
        {
            _erpDepartmentRepository = erpDepartmentRepository;
            _departmentRepository = departmentRepository;
            _departmentManager = departmentManager;
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
                bool firstExecute = true;
                while (true)
                {
                    if (!firstExecute)
                    {
                        await Task.Delay(360000).ConfigureAwait(false); //1小时更新一次
                    }

                    firstExecute = false;

                    using (var uow = _unitOfWorkManager.Begin())
                    {
                        try
                        {
                            var erpDepartments = await _erpDepartmentRepository.GetAllErpDepartmentsAsync(true, cancellationToken);
                            var departments = await _departmentRepository.GetAllAsync(true, cancellationToken);

                            if (erpDepartments == null || erpDepartments.Count == 0)
                            {
                                if (departments != null && departments.Count > 0)
                                {
                                    foreach (var department in departments)
                                        await _departmentRepository.DeleteAsync(department).ConfigureAwait(false);
                                }

                                await uow.CompleteAsync();
                                continue;
                            }

                            if (departments == null || departments.Count == 0)  //全部添加
                            {
                                foreach(var erpDepartment in erpDepartments)
                                {
                                    Department dept = await _departmentManager.CreateDepartmentAsync(
                                        erpDepartment.DEPT_ID, erpDepartment.DEPT_NAME).ConfigureAwait(false);

                                    await _departmentRepository.InsertAsync(dept).ConfigureAwait(false);
                                    await uow.SaveChangesAsync().ConfigureAwait(false);
                                }

                                await uow.CompleteAsync();
                                continue;
                            }
                            
                            if (erpDepartments.Count != departments.Count)
                            {
                                foreach (var department in departments)
                                {
                                    await _departmentRepository.DeleteAsync(department).ConfigureAwait(false);
                                    await uow.SaveChangesAsync().ConfigureAwait(false);
                                }

                                await uow.SaveChangesAsync().ConfigureAwait(false);

                                foreach (var erpDepartment in erpDepartments)
                                {
                                    Department dept = await _departmentManager.CreateDepartmentAsync(
                                        erpDepartment.DEPT_ID, erpDepartment.DEPT_NAME).ConfigureAwait(false);

                                    await _departmentRepository.InsertAsync(dept).ConfigureAwait(false);
                                    await uow.SaveChangesAsync().ConfigureAwait(false);
                                }

                                await uow.CompleteAsync();
                                continue;
                            }

                            bool isSame = true;
                            foreach(var department in departments)
                            {
                                var dpts = erpDepartments.Where(o => o.DEPT_ID == department.DepartmentCode &&
                                    o.DEPT_NAME == department.DepartmentName).ToList();
                                if (dpts == null || dpts.Count == 0)
                                {
                                    isSame = false;
                                    break;
                                }
                            }

                            if (!isSame)
                            {
                                foreach (var department in departments)
                                {
                                    await _departmentRepository.DeleteAsync(department).ConfigureAwait(false);
                                    await uow.SaveChangesAsync().ConfigureAwait(false);
                                }

                                await uow.SaveChangesAsync().ConfigureAwait(false);

                                foreach (var erpDepartment in erpDepartments)
                                {
                                    Department dept = await _departmentManager.CreateDepartmentAsync(
                                        erpDepartment.DEPT_ID, erpDepartment.DEPT_NAME).ConfigureAwait(false);

                                    await _departmentRepository.InsertAsync(dept).ConfigureAwait(false);
                                    await uow.SaveChangesAsync().ConfigureAwait(false);
                                }

                                await uow.CompleteAsync();
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            await uow.RollbackAsync().ConfigureAwait(false);
                            _logger.Error(ex.Message);
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
