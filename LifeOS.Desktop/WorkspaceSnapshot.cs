using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LifeOS.Desktop;

internal sealed class WorkspaceSnapshot
{
    private readonly IReadOnlyDictionary<string, int> _counts;

    private WorkspaceSnapshot(IReadOnlyDictionary<string, int> counts)
    {
        _counts = counts;
    }

    public static WorkspaceSnapshot Load()
    {
        TryLoadAssembly("LifeOS.Shared");

        Dictionary<string, int> counts = new(StringComparer.Ordinal)
        {
            ["agenda"] = LoadCount("LifeOS.Shared.Agenda.AgendaStorage"),
            ["follow-ups"] = LoadCount("LifeOS.Shared.FollowUps.FollowUpStorage"),
            ["integration-inbox"] = LoadCount("LifeOS.Shared.IntegrationInbox.IntegrationInboxStorage"),
            ["pay-later"] = LoadCount("LifeOS.Shared.PayLater.PayLaterStorage"),
            ["proof"] = LoadCount("LifeOS.Shared.ProofTracker.ProofStorage"),
            ["receipts"] = LoadCount("LifeOS.Shared.ReceiptEvidence.ReceiptEvidenceStorage"),
            ["relationships"] = LoadCount("LifeOS.Shared.RelationshipRadar.RelationshipRadarStorage"),
            ["timesheets"] = LoadCount("LifeOS.Shared.TimesheetEvidence.TimesheetEvidenceStorage"),
            ["weekly-close-out"] = LoadCount("LifeOS.Shared.WeeklyCloseOut.WeeklyCloseOutStorage"),
            ["work-pipeline"] = LoadCount("LifeOS.Shared.WorkPipeline.WorkPipelineStorage"),
            ["work-sessions"] = LoadCount("LifeOS.Shared.WorkSessions.WorkSessionStorage")
        };

        return new WorkspaceSnapshot(counts);
    }

    public string Resolve(WorkspaceMetricDefinition metric)
    {
        if (string.IsNullOrWhiteSpace(metric.MetricKey))
        {
            return metric.FallbackValue;
        }

        return _counts.TryGetValue(metric.MetricKey, out int count)
            ? count.ToString()
            : metric.FallbackValue;
    }

    private static void TryLoadAssembly(string assemblyName)
    {
        try
        {
            Assembly.Load(new AssemblyName(assemblyName));
        }
        catch
        {
            // The Desktop already references LifeOS.Shared. If loading is deferred or unavailable,
            // individual metrics fail closed to zero without affecting canonical module data.
        }
    }

    private static int LoadCount(string fullTypeName)
    {
        try
        {
            Type? storageType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType(fullTypeName, throwOnError: false))
                .FirstOrDefault(type => type is not null);

            MethodInfo? loadMethod = storageType?.GetMethod(
                "Load",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null);

            object? value = loadMethod?.Invoke(null, null);

            if (value is ICollection collection)
            {
                return collection.Count;
            }

            if (value is IEnumerable enumerable)
            {
                int count = 0;
                foreach (object? _ in enumerable)
                {
                    count++;
                }

                return count;
            }
        }
        catch
        {
            // Summary metrics are non-authoritative. Canonical data remains inside its module.
        }

        return 0;
    }
}
