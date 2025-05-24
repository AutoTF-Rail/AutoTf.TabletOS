using System.Text;
using System.Text.Json;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Enums;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services;

public class TrainControlService : ITrainControlService
{
	private readonly string _baseUrl = "http://192.168.1.1/control";

	public async Task<bool> IsEasyControlAvailable() => await HttpHelper.SendGet<bool>(_baseUrl + "/isEasyControlAvailable", false);

	public async Task<int> GetLeverCount() => await HttpHelper.SendGet<int>(_baseUrl + "/levercount", false);

	public async Task<double> GetLeverPosition(int leverIndex) => await HttpHelper.SendGet<double>(_baseUrl + "/leverPosition", false);

	public async Task<LeverType> GetLeverType(int leverIndex) => await HttpHelper.SendGet<LeverType>(_baseUrl + "/leverPosition", false);

	// TODO: Reverse UI change if this fails/request the current state on the server to update the UI again
	public async Task<bool> SetLever(LeverSetModel leverModel) => await HttpHelper.SendPost(_baseUrl + "/setLever", new StringContent(JsonSerializer.Serialize(leverModel), Encoding.UTF8, "application/json"));

	public async Task<bool> EasyControl(int power) => await HttpHelper.SendPost(_baseUrl + "/easyControl", new StringContent(JsonSerializer.Serialize(power), Encoding.UTF8, "application/json"));

	public async Task<bool> EmergencyBrake() => await HttpHelper.SendPost(_baseUrl + "/emergencybrake", new StringContent("", Encoding.UTF8, "application/json"));
}