﻿using AutoMapper;
using CloudStorage.Application.Users.Interfaces;
using CloudStorage.Application.Users.Models;
using CloudStorage.Application.Users.Models.UserBasicInfo;
using CloudStorage.Domain.Core.Transactions;
using CloudStorage.Domain.Messages.SagaMessages.CreateAccountSaga;
using CloudStorage.Domain.Users.Entities;

namespace CloudStorage.Application.Users.Services;

public class UserSagaService : ISagaService<CreateUserPayload, CreateUserCompensation>
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public UserSagaService(IUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }
    public async Task<(Guid RecordUuid, CreateUserCompensation Rollback)> ExecuteAsync(
        SagaServiceMessage<CreateUserPayload> request)
    {
        var recordUuid = await _userService.CreateUserAsync(_mapper.Map<NewUserInfo>(request.Payload));
        return (recordUuid, new CreateUserCompensation() { Uuid = recordUuid });
    }
    public async Task RollbackAsync(CreateUserCompensation rollback)
    {
        await _userService.DeleteUserAsync(rollback.Uuid);
    }
}