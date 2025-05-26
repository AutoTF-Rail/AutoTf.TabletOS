using System.Text;
using System.Text.Json;
using AutoTf.CentralBridge.Shared.Models;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Enums;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services;

public class TrainControlService : ITrainControlService
{
	private readonly string _baseUrl = "http://192.168.1.1/control";

	public async Task<Result<bool>> IsEasyControlAvailable() => await HttpHelper.SendGet<bool>(_baseUrl + "/isEasyControlAvailable");

	public async Task<Result<int>> GetLeverCount() => await HttpHelper.SendGet<int>(_baseUrl + "/levercount");

	public async Task<Result<double>> GetLeverPosition(int leverIndex) => await HttpHelper.SendGet<double>(_baseUrl + "/leverPosition");

	public async Task<Result<LeverType>> GetLeverType(int leverIndex) => await HttpHelper.SendGet<LeverType>(_baseUrl + "/leverPosition");

	// TODO: Reverse UI change if this fails/request the current state on the server to update the UI again
	public async Task<Result> SetLever(LeverSetModel leverModel) => await HttpHelper.SendPost(_baseUrl + "/setLever", new StringContent(JsonSerializer.Serialize(leverModel), Encoding.UTF8, "application/json"));

	public async Task<Result> EasyControl(int power) => await HttpHelper.SendPost(_baseUrl + "/easyControl", new StringContent(JsonSerializer.Serialize(power), Encoding.UTF8, "application/json"));

	public async Task<Result> EmergencyBrake() => await HttpHelper.SendPost(_baseUrl + "/emergencybrake", new StringContent("", Encoding.UTF8, "application/json"));
}