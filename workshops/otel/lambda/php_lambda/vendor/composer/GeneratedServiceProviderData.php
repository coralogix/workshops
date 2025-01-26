<?php declare(strict_types=1);
namespace Nevay\SPI;

/**
 * @internal 
 */
final class GeneratedServiceProviderData {

    public const VERSION = 1;

    /**
     * @param class-string $service
     * @return list<class-string>
     */
    public static function providers(string $service): array {
        return match ($service) {
            default => [],
            \OpenTelemetry\API\Instrumentation\AutoInstrumentation\HookManagerInterface::class => [
                ...((true && (($r = new \Nevay\SPI\ServiceProviderDependency\ExtensionDependency('opentelemetry', '^1.0'))->hash() === '1.1.2' || $r->isSatisfied())) ? [
                \OpenTelemetry\API\Instrumentation\AutoInstrumentation\ExtensionHookManager::class, // open-telemetry/api 1.2.0 (extra.spi)
                ] : []),
            ],
        };
    }
}