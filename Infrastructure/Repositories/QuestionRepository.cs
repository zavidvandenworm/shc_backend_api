using System.Data;
using Common.Dto;
using Common.Models;
using Dapper;

namespace Infrastructure.Repositories;

public class QuestionRepository
{
    private readonly IDbConnection _dbConnection;
    private readonly UserRepository _userRepository;
    public QuestionRepository(IDbConnection dbConnection, UserRepository userRepository)
    {
        _dbConnection = dbConnection;
        _userRepository = userRepository;
    }

    public async Task<List<Question>> GetAll(string uniqueLink, int versionId)
    {
        var user = await _userRepository.GetFromUniqueLink(uniqueLink);
        const string sql = @"
                    SELECT q.*
                    FROM question q
                    JOIN healthcheck hc ON q.healthcheck_id = hc.id
                    JOIN invitation_link il ON hc.id = il.healthcheck_id
                    WHERE il.uniqueLink = @uniqueLink AND q.version_id = @versionId
                    ORDER BY q.priority";

        var parameters = new DynamicParameters();
        parameters.Add("@uniqueLink", uniqueLink);
        parameters.Add("@versionId", versionId);

        var questions = await _dbConnection.QueryAsync<Question>(sql, parameters);

        var questionIds = questions.Select(q => q.Id);
        foreach (var VARIABLE in questionIds)
        {
            Console.Out.WriteLine(VARIABLE);
        }
        var answers = await GetAllAnswers(questionIds.ToArray(), versionId, user.Id);

        foreach (var answer in answers)
        {
            var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question is null)
            {
                continue;
            }
            question.Answer = answer;
        }

        return questions.ToList();
    }

    private async Task<IEnumerable<Answer>> GetAllAnswers(int[] questionIds, int versionId, int userId)
    {
        const string sql =
            @"SELECT *, question_id AS questionId, flagged as isFlagged FROM answer WHERE question_id IN @questionIds";
        var parameters = new
        {
            userId = userId,
            versionId = versionId,
            questionIds = questionIds
        };
        var answers = await _dbConnection.QueryAsync<Answer>(sql, parameters);
        Console.Out.WriteLine(answers.Count() + " answers");
        return answers;
    }

    public async Task<Answer?> GetAnswer(int questionId, int userId)
    {
        const string sql = @"SELECT * FROM answer WHERE question_id = @questionId AND user_id = @userId";
        var parameters = new DynamicParameters();
        parameters.Add("@questionId", questionId);
        parameters.Add("@userId", userId);
        Answer? answer = await _dbConnection.QuerySingleOrDefaultAsync<Answer?>(sql, parameters);
        return answer;
    }
    
    
    public async Task UpdateOrInsertAnswer(int questionId, int userId, AnswerDto answer)
    {
        if (await GetAnswer(questionId, userId) is null)
        {
            const string sql = @"INSERT INTO answer (question_id, user_id, answerColor, comment, flagged)" +
                               "VALUES (@questionId, @userId, @answerColor, @comment, @isFlagged)";
            var parameters = new DynamicParameters();
            parameters.Add("@questionId", questionId);
            parameters.Add("@userId", userId);
            parameters.Add("@answerColor", answer.AnswerColor);
            parameters.Add("@comment", answer.Comment);
            parameters.Add("@isFlagged", answer.IsFlagged);

            await _dbConnection.ExecuteAsync(sql, parameters);
        }
        else
        {
            Console.Out.WriteLine("Updating answer...");
            const string sql =
                @"UPDATE answer SET answerColor = @answerColor, comment = @comment, flagged = @isFlagged WHERE question_id = @questionId AND user_id = @userId";
            
            var parameters = new DynamicParameters();
            parameters.Add("@questionId", questionId);
            parameters.Add("@userId", userId);
            parameters.Add("@answerColor", answer.AnswerColor);
            parameters.Add("@comment", answer.Comment);
            parameters.Add("@isFlagged", answer.IsFlagged);

            await _dbConnection.ExecuteAsync(sql, parameters);
        }
    }

    public async Task UpdateAnswer(int questionId, int userId, Answer answer)
    {
        const string sql = @"UPDATE answer
                                        SET answerColor = @answerColor,
                                        comment = @comment,
                                        flagged = @isFlagged
                                        WHERE question_id = @questionId AND user_id = @userId;";
        var parameters = new DynamicParameters();
        parameters.Add("@questionId", questionId);
        parameters.Add("@userId", userId);
        parameters.Add("@answerColor", answer.AnswerColor);
        parameters.Add("@comment", answer.Comment);
        parameters.Add("@isFlagged", answer.IsFlagged);

        await _dbConnection.ExecuteAsync(sql, parameters);
    }

    public async Task InsertAnswer(int questionId, int userId, Answer answer)
    {
        const string sql = @"INSERT INTO answer (question_id, user_id, answerColor, comment, flagged)" +
                       "VALUES (@questionId, @userId, @answerColor, @comment, @isFlagged)";
        var parameters = new DynamicParameters();
        parameters.Add("@questionId", questionId);
        parameters.Add("@userId", userId);
        parameters.Add("@answerColor", answer.AnswerColor);
        parameters.Add("@comment", answer.Comment);
        parameters.Add("@isFlagged", answer.IsFlagged);

        await _dbConnection.ExecuteAsync(sql, parameters);
    }

    public async Task DeleteAnswer(int questionId, int userId)
    {
        const string sql = @"DELETE FROM answer WHERE question_id = @questionId";
        var parameters = new DynamicParameters();
        parameters.Add("@questionId", questionId);
        await _dbConnection.ExecuteAsync(sql, parameters);
    }
}