﻿using Scribe.Data.Model;

namespace Scribe.UI.Events;

public record DocumentCreatedEvent(Document CreatedDocument) : IEvent;