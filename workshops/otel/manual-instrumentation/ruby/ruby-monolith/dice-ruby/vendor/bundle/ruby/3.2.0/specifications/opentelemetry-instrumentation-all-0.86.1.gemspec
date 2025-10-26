# -*- encoding: utf-8 -*-
# stub: opentelemetry-instrumentation-all 0.86.1 ruby lib

Gem::Specification.new do |s|
  s.name = "opentelemetry-instrumentation-all".freeze
  s.version = "0.86.1"

  s.required_rubygems_version = Gem::Requirement.new(">= 0".freeze) if s.respond_to? :required_rubygems_version=
  s.metadata = { "bug_tracker_uri" => "https://github.com/open-telemetry/opentelemetry-ruby-contrib/issues", "changelog_uri" => "https://rubydoc.info/gems/opentelemetry-instrumentation-all/0.86.1/file/CHANGELOG.md", "documentation_uri" => "https://rubydoc.info/gems/opentelemetry-instrumentation-all/0.86.1", "source_code_uri" => "https://github.com/open-telemetry/opentelemetry-ruby-contrib/tree/main/instrumentation/all" } if s.respond_to? :metadata=
  s.require_paths = ["lib".freeze]
  s.authors = ["OpenTelemetry Authors".freeze]
  s.date = "2025-10-22"
  s.description = "All-in-one instrumentation bundle for the OpenTelemetry framework".freeze
  s.email = ["cncf-opentelemetry-contributors@lists.cncf.io".freeze]
  s.homepage = "https://github.com/open-telemetry/opentelemetry-ruby-contrib".freeze
  s.licenses = ["Apache-2.0".freeze]
  s.post_install_message = "".freeze
  s.required_ruby_version = Gem::Requirement.new(">= 3.2".freeze)
  s.rubygems_version = "3.4.20".freeze
  s.summary = "All-in-one instrumentation bundle for the OpenTelemetry framework".freeze

  s.installed_by_version = "3.4.20" if s.respond_to? :installed_by_version

  s.specification_version = 4

  s.add_runtime_dependency(%q<opentelemetry-instrumentation-active_model_serializers>.freeze, ["~> 0.24.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-anthropic>.freeze, ["~> 0.3.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-aws_lambda>.freeze, ["~> 0.6.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-aws_sdk>.freeze, ["~> 0.11.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-bunny>.freeze, ["~> 0.24.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-concurrent_ruby>.freeze, ["~> 0.24.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-dalli>.freeze, ["~> 0.29.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-delayed_job>.freeze, ["~> 0.25.1"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-ethon>.freeze, ["~> 0.25.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-excon>.freeze, ["~> 0.26.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-faraday>.freeze, ["~> 0.30.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-grape>.freeze, ["~> 0.5.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-graphql>.freeze, ["~> 0.31.1"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-grpc>.freeze, ["~> 0.4.1"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-gruf>.freeze, ["~> 0.5.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-http>.freeze, ["~> 0.27.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-http_client>.freeze, ["~> 0.26.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-httpx>.freeze, ["~> 0.5.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-koala>.freeze, ["~> 0.23.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-lmdb>.freeze, ["~> 0.25.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-mongo>.freeze, ["~> 0.25.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-mysql2>.freeze, ["~> 0.31.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-net_http>.freeze, ["~> 0.26.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-pg>.freeze, ["~> 0.32.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-que>.freeze, ["~> 0.11.1"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-racecar>.freeze, ["~> 0.6.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-rack>.freeze, ["~> 0.29.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-rails>.freeze, ["~> 0.39.1"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-rake>.freeze, ["~> 0.5.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-rdkafka>.freeze, ["~> 0.9.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-redis>.freeze, ["~> 0.28.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-resque>.freeze, ["~> 0.8.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-restclient>.freeze, ["~> 0.26.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-ruby_kafka>.freeze, ["~> 0.24.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-sidekiq>.freeze, ["~> 0.28.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-sinatra>.freeze, ["~> 0.28.0"])
  s.add_runtime_dependency(%q<opentelemetry-instrumentation-trilogy>.freeze, ["~> 0.64.0"])
end
