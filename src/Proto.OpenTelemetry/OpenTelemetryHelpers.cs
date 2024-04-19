﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Proto.Extensions;

namespace Proto.OpenTelemetry;

internal static class OpenTelemetryHelpers
{
    private static readonly ActivitySource ActivitySource = new(ProtoTags.ActivitySourceName);

    public static void DefaultSetupActivity(Activity _, object __)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Activity? BuildStartedActivity(
        ActivityContext parent,
        string source,
        string verb,
        object message,
        ActivitySetup activitySetup,
        ActivityKind activityKind = ActivityKind.Internal
    )
    {
        var messageType = message.GetMessageTypeName();
        return BuildStartedActivity(parent, source, verb, messageType, activitySetup, activityKind);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Activity? BuildStartedActivity(
        ActivityContext parent,
        string source,
        string verb,
        string message,
        ActivitySetup activitySetup,
        ActivityKind activityKind = ActivityKind.Internal
    )
    {
        

        var name = $"Proto {source}.{verb} {message}";
        var tags = new[] { new KeyValuePair<string, object?>(ProtoTags.MessageType, message) };
        var activity = ActivitySource.StartActivity(name, activityKind, parent, tags);

        if (activity is not null)
        {
            activitySetup(activity, message!);
        }

        return activity;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Activity? BuildStartedSpawnActivity(
        ActivityContext parent,
        string source,
        string verb,
        string actorName,
        ActivitySetup activitySetup,
        ActivityKind activityKind = ActivityKind.Internal
    )
    {
        
        var name = $"Proto {source}.{verb} {actorName}";
        var tags = Array.Empty<KeyValuePair<string, object?>>();
        var activity = ActivitySource.StartActivity(name, activityKind, parent, tags);

        if (activity is not null)
        {
            activitySetup(activity, actorName!);
        }

        return activity;
    }

    public static IEnumerable<KeyValuePair<string, string>> GetPropagationHeaders(this ActivityContext activityContext)
    {
        var context = new List<KeyValuePair<string, string>>();

        Propagators.DefaultTextMapPropagator.Inject(new PropagationContext(activityContext, Baggage.Current), context,
            AddHeader);

        return context;
    }

    public static PropagationContext ExtractPropagationContext(this MessageHeader headers) =>
        Propagators.DefaultTextMapPropagator.Extract(default, headers.ToDictionary(),
            (dictionary, key) => dictionary.TryGetValue(key, out var value) ? new[] { value } : Array.Empty<string>()
        );

    private static void AddHeader(List<KeyValuePair<string, string>> list, string key, string value) =>
        list.Add(new KeyValuePair<string, string>(key, value));
}