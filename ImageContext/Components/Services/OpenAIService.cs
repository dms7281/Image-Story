using ImageMagick;
using Microsoft.AspNetCore.Components.Forms;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace ImageContext.Components.Services;

public class OpenAIService(IConfiguration config)
{
    private OpenAIAPI _api = new(config["OpenAIKey"]);
    
    public async Task<string?> AddressCorrection(string? originalAddress, byte[] imageBytes)
    {
        Console.WriteLine("Open AI Address Correction Started");
        
        // Take original address and correct it based on the image
        var chat = _api.Chat.CreateConversation();
        chat.RequestParameters.Model = "gpt-4o-mini";
        //chat.RequestParameters.Temperature = 0;
        chat.RequestParameters.TopP = 0.01;
        chat.AppendSystemMessage("You will be given an image of a location and the name of a place with an address. " +
                                 "If the name of the place does not match the image, please fix the name of the place and the address. " +
                                 "Output only the name in the form: place name, the rest of address. " +
                                 "If the name of the place DOES match the image, return the same address");
        
        chat.AppendUserInput(originalAddress, ChatMessage.ImageInput.FromImageBytes(imageBytes, detail:"low"));
        var response = await chat.GetResponseFromChatbotAsync();
        chat.Messages.Clear();
        return response;
    }
    
    public async Task<string?> HistoryOfAddress(string? address)
    {
        Console.WriteLine("Open AI History of Address Started");
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
        return response;
    }
}