using Dapper;
using System.Collections.Generic;
using MVCCore.Models;
using System.Threading.Tasks;
using System;

namespace MVCCore.Helpers;

public class ManhwaService
{
    private readonly DataContext dataContext;

    public ManhwaService(DataContext dataContext)
    {
        this.dataContext = dataContext;
    }

    public async Task<List<ManhwaScoreModel>> GetScores()
    {
        var query = "select * from manhwaScore";

        using var conn = dataContext.CreateConnection();

        var list = await conn.QueryAsync<ManhwaScoreModel>(new CommandDefinition(commandText: query)).ToList();

        return list;
    }

    public async Task AddScore(string name, int score)
    {
        if (name.Length == 0) throw new ArgumentException("Please specify name", nameof(name));
        if (!(score >= 1 && score <= 5)) throw new ArgumentException("Score must be [1-5]", nameof(score));

        var query = "insert into manhwaScore(name,score) values(@name, @score)";
        using var conn = dataContext.CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(query, parameters: new
        {
            name,
            score,
        }));
    }

    public async Task<List<ManhwaChapterModel>> GetChapters(string manhwaName)
    {
        if (manhwaName.Length == 0) throw new ArgumentException("Please specify name", nameof(manhwaName));

        var query = "select * from manhwaChapter where manhwaName = @manhwaName";

        using var conn = dataContext.CreateConnection();
        var list = await conn.QueryAsync<ManhwaChapterModel>(new CommandDefinition(query, parameters: new
        {
            manhwaName,
        })).ToList();
        return list;
    }

    public async Task AddChapter(string manhwaName, string chapterName)
    {
        if (manhwaName.Length == 0) throw new ArgumentException("Please specify manhwa name", nameof(manhwaName));
        if (chapterName.Length == 0) throw new ArgumentException("Please specify chapter name", nameof(chapterName));

        var query = "insert into manhwaChapter(manhwaName,chapterName) values(@manhwaName, @chapterName)";
        using var conn = dataContext.CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(query, parameters: new
        {
            manhwaName,
            chapterName,
        }));
    }
}