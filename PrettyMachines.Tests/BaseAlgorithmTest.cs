using PrettyMachines.Algorithms.Abstract;
using Xunit.Abstractions;


namespace PrettyMachines.Tests;

public abstract class BaseAlgorithmTest(ITestOutputHelper output)
{
    protected void CheckAlgorithm(string? expectedOutput, TerminationStatus? expectedStatus, IAlgorithm<string> algorithm, string input, AlgorithmCancellation? cancellation = null)
    {
        cancellation ??= new AlgorithmCancellation(50);
        var result = algorithm.Execute(input, cancellation.Value, verbose: true);

        output.WriteLine($"Algorithm traces [{result.Trace.Count}]");
        for (var i = 0; i < result.Trace.Count; i++)
        {
            output.WriteLine(result.Trace[i]);
        }
        
        TerminationStatus[] validStatuses = expectedStatus.HasValue
            ? [expectedStatus.Value]
            : [TerminationStatus.Stuck, TerminationStatus.Success];
        
        if ((expectedOutput == null && result.Output != input || expectedOutput != null && result.Output == expectedOutput) && 
            validStatuses.Contains(result.Termination))
            return;
        
        Assert.Fail(
            $"""
             {algorithm.GetType().Name} execution has ended with unexpected result
             - input:       '{input}'
             - act. output: '{result.Output}'
             - exp. output: {(expectedOutput == null ? "!= input" : $"'{expectedOutput}'")}
             - act. status: '{result.Termination}'
             - exp. status: '{expectedStatus}'
             - steps: {result.Steps} max step: {(cancellation.Value.MaxSteps == 0 ? "token" : cancellation.Value.MaxSteps.ToString())}
             """
        );
    }
    
    protected void CheckAlgorithm(TerminationStatus? expectedStatus, IAlgorithm<string> algorithm, string input, AlgorithmCancellation? cancellation = null)
    {
        cancellation ??= new AlgorithmCancellation(50);
        var result = algorithm.Execute(input, cancellation.Value, verbose: true);

        output.WriteLine($"Algorithm traces [{result.Trace?.Count ?? 0}]");
        for (var i = 0; i < result.Trace!.Count; i++)
        {
            output.WriteLine(result.Trace[i]);
        }
        
        TerminationStatus[] validStatuses = expectedStatus.HasValue
            ? [expectedStatus.Value]
            : [TerminationStatus.Stuck, TerminationStatus.Success];
        
        if (validStatuses.Contains(result.Termination))
            return;
        
        Assert.Fail(
            $"""
             {algorithm.GetType().Name} execution has ended with unexpected result
             - input:       '{input}'
             - act. output: '{result.Output}'
             - exp. output: "_any_"
             - act. status: '{result.Termination}'
             - exp. status: '{expectedStatus}'
             - steps: {result.Steps} max step: {(cancellation.Value.MaxSteps == 0 ? "token" : cancellation.Value.MaxSteps.ToString())}
             """
        );
    }
}