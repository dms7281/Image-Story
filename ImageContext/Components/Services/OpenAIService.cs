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
            addressToInput += "Address: " + s;
            //Console.WriteLine(addressToInput);
        }
        
        // Take original address and correct it based on the image
        var chat = _api.Chat.CreateConversation();
        chat.RequestParameters.Model = "gpt-4o-mini";
        //chat.RequestParameters.Temperature = 0;
        chat.RequestParameters.TopP = 0.05;
        chat.AppendSystemMessage("You will be given an image of a location and a address that is near the location, pictured in the photo, or IS the location pictured in the photo." +
                                 "If the location, pictured in the photo is different from the given address, then correct the address." +
                                 "For example: You are given the address \"Stadium Dr & Hagerstown Ln, College Park, MD 20742\", but the location pictured is a stadium. Since Maryland Stadium is near the given address, " +
                                 "then the correct address would actually be Maryland Stadium, College Park, MD 20742. " +
                                 "Output only the correct address.");
        
        chat.AppendUserInput(originalAddresses[0], ChatMessage.ImageInput.FromImageBytes(imageBytes, detail:"low"));
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
    
    public async Task<string> CreateOptimalSearchTerm(string? address)
    {
        // string searchTermString = "";
        // foreach (string s in searchTerms)
        // {
        //     searchTermString += " " + s;
        // }
        
        // Take original address and correct it based on the image
        var chat = _api.Chat.CreateConversation();
        chat.RequestParameters.Model = "gpt-4o-mini";
        //chat.RequestParameters.Temperature = 0;
        chat.RequestParameters.TopP = 0.01;
        chat.AppendSystemMessage("You will be given an address, pick the part of the address that would be most ideal as a search term. " +
                                 "Focus on actual places, if there you aren't given any actual places, focus on the city and region. " +
                                 "For example, if given Beaver Stadium, University Park, PA 16802. Then you should return: Beaver Stadium. " +
                                 "But if you were given 1234 North Street, Cleveland, Ohio, 12345. Then you should return: Cleveland Ohio.");
        
        chat.AppendUserInput(address);
        var response = await chat.GetResponseFromChatbotAsync();
        chat.Messages.Clear();
        
        Console.WriteLine("Open AI API: Address Correction Successful");
        
        return response;
    }
}