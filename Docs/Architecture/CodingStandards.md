# Coding standards, lifecycle и Unity-ассеты

[AGENTS](../../AGENTS.md) · [Overview](Overview.md) · [Testing](Testing.md)

## C# и структура

- Английские имена: PascalCase для типов/методов/свойств, camelCase для локальных, _camelCase для private fields. 4 пробела, UTF-8, LF, один основной public тип на файл; учитывать .editorconfig.
- Узкие публичные контракты, явные единицы (DurationTicks, Capacity), без магических чисел и необоснованных reflection/unsafe. Комментарии объясняют причину. Документация на русском допустима; commit messages и имена файлов английские.

Не вводить интерфейс для каждого класса, service locator, глобальные mutable singleton или универсальную шину событий. Один явный composition root на сессию. Соблюдать границы сборок из Overview; editor-only код и тесты не попадают в runtime. Не добавлять asmdef/пустой namespace только для структуры.

ScriptableObject — immutable input для конкретной сессии. Баланс и настройки в definitions/config, не разбросаны по системам. Учитывать .editorconfig, если появится. Комментарии объясняют причину, инвариант и порядок.

## Unity lifecycle и Domain Reload

В Unity 6.6 для новых проектов Domain Reload при входе в Play Mode может быть отключён по умолчанию. Проверить фактические Enter Play Mode settings после импорта. Не полагаться на очистку static state редактором: явный Start/Dispose сессии, создание/остановка clocks, подписок и jobs, Dispose native memory, очистка view registry/pools. При смене сцены не создавать второй активный session owner. Учитывать отключение Scene Reload, если включено. Проверять последовательные Play Mode A → stop → A.

Остаточные статические поля, которые действительно нужны, очищать при SubsystemRegistration, где применимо:

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
private static void ResetStaticState()
{
    // Reset only the static state owned by this type.
}
```

Это шаблон, не готовая система. Предпочитать instance-owned state. Не добавлять пустой ResetStaticState ради соблюдения регламента. Подписки снимаются владельцем; сброс полей не заменяет Dispose. Сериализуемые ссылки: [SerializeField] private, проверять конфиг и обязательные зависимости. Не полагаться на случайный порядок Awake/Start/Update.

## Assets и Git

- Visible Meta Files + Force Text. Сохранять GUID, перемещать ассеты вместе с .meta предпочтительно через Editor. Не придумывать GUID для существующих объектов и не править крупный scene/prefab YAML без проверки.
- В Git: Assets, Packages, ProjectSettings и .meta. Не коммитить Library, Temp, Obj, Logs, UserSettings, builds и IDE-кэш. Следовать .gitignore; Git LFS для согласованных крупных бинарных ассетов.
- Сторонние ассеты/пакеты только с понятной лицензией. Без секретов, токенов, личных данных и локальных абсолютных путей.

Проверить реальные ProjectSettings, не предполагать настройки включёнными. Git LFS согласовать до добавления крупных файлов; проверить .gitattributes. Не добавлять коммерческие ассеты в публичный репозиторий без права распространения. Не переписывать .gitignore без задачи. Не менять версию Unity, пакеты или лицензию без согласования.

Рабочий процесс: небольшие изменения, git diff/status, тематические коммиты. Значимые изменения — рабочая ветка/PR; без force-push, чужих изменений и изменений истории. Крупные архитектурные решения фиксировать кратким ADR в `Docs/Architecture/Decisions/` по [DocumentationPolicy](../DocumentationPolicy.md); для обычного класса ADR не нужен.

Документация для сверки: [Unity Manual 6.6](https://docs.unity3d.com/6000.6/Documentation/Manual/index.html).
