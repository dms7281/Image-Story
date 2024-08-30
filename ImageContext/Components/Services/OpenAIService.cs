using ImageMagick;
using Microsoft.AspNetCore.Components.Forms;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace ImageContext.Components.Services;

public class OpenAIService(IConfiguration configuration)
{
    private readonly OpenAIAPI _api = new(configuration["OpenAIKey"]);

    public async Task<string> AddressCorrection(List<string?> originalAddresses, byte[] imageBytes)
    {
        string addressToInput = "";
        foreach (string s in originalAddresses)
        {
            addressToInput += " " + s;
        }
        
        // Take original address and correct it based on the image
        var chat = _api.Chat.CreateConversation();
        chat.RequestParameters.Model = "gpt-4o-mini";
        //chat.RequestParameters.Temperature = 0;
        chat.RequestParameters.TopP = 0.01;
        chat.AppendSystemMessage("You will be given an image of a location and a list of address that are near this photo or are in the photo. " +
                                 "Consider all of these addresses and the image to determine the most accurate address." +
                                 "Output only the correct address.");
        
        chat.AppendUserInput(addressToInput, ChatMessage.ImageInput.FromImageBytes(imageBytes, detail:"low"));
        var response = await chat.GetResponseFromChatbotAsync();
        chat.Messages.Clear();
        
        Console.WriteLine("Open AI API: Address Correction Successful");
        
        return response;
    }
    
    public async Task<string> HistoryOfAddress(string? address)
    {
        // Take address and generate 1000 character string about history of place and address
        var chat = _api.Chat.CreateConversation();
        chat.RequestParameters.Model = "gpt-4o-mini";
        //chat.RequestParameters.Temperature = 0;
        chat.RequestParameters.TopP = 0.01;
        chat.AppendSystemMessage("You will be given an address, summarize the history of this address. " +
                                 "If the address has a named place, focus on that, otherwise focus on the city. " +
                                 "1000 characters or less");
        chat.AppendUserInput(address);
        var response = await chat.GetResponseFromChatbotAsync();
        chat.Messages.Clear();
        
        Console.WriteLine("Open AI API: History of Address Successful");
        
        return response;
    }
}