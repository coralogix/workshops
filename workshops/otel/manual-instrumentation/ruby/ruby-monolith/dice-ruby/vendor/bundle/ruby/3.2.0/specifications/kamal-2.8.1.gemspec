# -*- encoding: utf-8 -*-
# stub: kamal 2.8.1 ruby lib

Gem::Specification.new do |s|
  s.name = "kamal".freeze
  s.version = "2.8.1"

  s.required_rubygems_version = Gem::Requirement.new(">= 0".freeze) if s.respond_to? :required_rubygems_version=
  s.require_paths = ["lib".freeze]
  s.authors = ["David Heinemeier Hansson".freeze]
  s.date = "1980-01-02"
  s.email = "dhh@hey.com".freeze
  s.executables = ["kamal".freeze]
  s.files = ["bin/kamal".freeze]
  s.homepage = "https://github.com/basecamp/kamal".freeze
  s.licenses = ["MIT".freeze]
  s.rubygems_version = "3.4.20".freeze
  s.summary = "Deploy web apps in containers to servers running Docker with zero downtime.".freeze

  s.installed_by_version = "3.4.20" if s.respond_to? :installed_by_version

  s.specification_version = 4

  s.add_runtime_dependency(%q<activesupport>.freeze, [">= 7.0"])
  s.add_runtime_dependency(%q<sshkit>.freeze, [">= 1.23.0", "< 2.0"])
  s.add_runtime_dependency(%q<net-ssh>.freeze, ["~> 7.3"])
  s.add_runtime_dependency(%q<thor>.freeze, ["~> 1.3"])
  s.add_runtime_dependency(%q<dotenv>.freeze, ["~> 3.1"])
  s.add_runtime_dependency(%q<zeitwerk>.freeze, [">= 2.6.18", "< 3.0"])
  s.add_runtime_dependency(%q<ed25519>.freeze, ["~> 1.4"])
  s.add_runtime_dependency(%q<bcrypt_pbkdf>.freeze, ["~> 1.0"])
  s.add_runtime_dependency(%q<concurrent-ruby>.freeze, ["~> 1.2"])
  s.add_runtime_dependency(%q<base64>.freeze, ["~> 0.2"])
  s.add_development_dependency(%q<debug>.freeze, [">= 0"])
  s.add_development_dependency(%q<mocha>.freeze, [">= 0"])
  s.add_development_dependency(%q<railties>.freeze, [">= 0"])
end
